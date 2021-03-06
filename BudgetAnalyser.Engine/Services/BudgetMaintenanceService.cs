﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to provide maintenance support for budget models and collections.
    ///     This class is stateful and is intended to be used as a single instance.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetMaintenanceService : IBudgetMaintenanceService, ISupportsModelPersistence
    {
        private readonly IBudgetRepository budgetRepository;
        private readonly IDashboardService dashboardService;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetMaintenanceService" /> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">budgetRepository</exception>
        public BudgetMaintenanceService(
            [NotNull] IBudgetRepository budgetRepository,
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] ILogger logger,
            [NotNull] IDashboardService dashboardService)
        {
            if (budgetRepository == null)
            {
                throw new ArgumentNullException("budgetRepository");
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (dashboardService == null)
            {
                throw new ArgumentNullException("dashboardService");
            }

            this.budgetRepository = budgetRepository;
            this.logger = logger;
            this.dashboardService = dashboardService;
            BudgetBucketRepository = bucketRepo;
            CreateNewBudgetCollection();
        }

        public event EventHandler Closed;
        public event EventHandler NewDataSourceAvailable;
        public event EventHandler Saved;
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;
        public event EventHandler<ValidatingEventArgs> Validating;
        public IBudgetBucketRepository BudgetBucketRepository { get; private set; }
        public BudgetCollection Budgets { get; private set; }

        public ApplicationDataType DataType
        {
            get { return ApplicationDataType.Budget; }
        }

        public int LoadSequence
        {
            get { return 5; }
        }

        public BudgetModel CloneBudgetModel(BudgetModel sourceBudget, DateTime newBudgetEffectiveFrom)
        {
            if (sourceBudget == null)
            {
                throw new ArgumentNullException("sourceBudget");
            }

            if (newBudgetEffectiveFrom <= sourceBudget.EffectiveFrom)
            {
                throw new ArgumentException("The effective date of the new budget must be later than the other budget.", "newBudgetEffectiveFrom");
            }

            if (newBudgetEffectiveFrom <= DateTime.Today)
            {
                throw new ArgumentException("The effective date of the new budget must be a future date.", "newBudgetEffectiveFrom");
            }

            var validationMessages = new StringBuilder();
            if (!sourceBudget.Validate(validationMessages))
            {
                throw new ValidationWarningException(
                    string.Format(CultureInfo.CurrentCulture, "The source budget is currently in an invalid state, unable to clone it at this time.\n{0}", validationMessages));
            }

            var newBudget = new BudgetModel
            {
                EffectiveFrom = newBudgetEffectiveFrom,
                Name = string.Format(CultureInfo.CurrentCulture, "Copy of {0}", sourceBudget.Name)
            };
            newBudget.Update(CloneBudgetIncomes(sourceBudget), CloneBudgetExpenses(sourceBudget));

            if (!newBudget.Validate(validationMessages))
            {
                throw new InvalidOperationException("New cloned budget is invalid and the source budget is not. Code Error.\n" + validationMessages);
            }

            Budgets.Add(newBudget);
            this.budgetRepository.SaveAsync();
            return newBudget;
        }

        public void Close()
        {
            CreateNewBudgetCollection();
            EventHandler handler = Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public async Task CreateAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase.BudgetCollectionStorageKey.IsNothing())
            {
                throw new ArgumentNullException("applicationDatabase");
            }

            await this.budgetRepository.CreateNewAndSaveAsync(applicationDatabase.BudgetCollectionStorageKey);
            await LoadAsync(applicationDatabase);
        }

        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase == null)
            {
                throw new ArgumentNullException("applicationDatabase");
            }

            Budgets = await this.budgetRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.BudgetCollectionStorageKey));
            EventHandler handler = NewDataSourceAvailable;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            this.dashboardService.NotifyOfDependencyChange<IBudgetBucketRepository>(BudgetBucketRepository);
        }

        public async Task SaveAsync(IDictionary<ApplicationDataType, object> contextObjects)
        {
            EnsureAllBucketsUsedAreInBucketRepo();

            var messages = new StringBuilder();
            if (Budgets.Validate(messages))
            {
                await this.budgetRepository.SaveAsync();
                EventHandler savedHandler = Saved;
                if (savedHandler != null)
                {
                    savedHandler(this, EventArgs.Empty);
                }
                return;
            }

            this.logger.LogWarning(l => l.Format("BudgetMaintenanceService.Save: unable to save due to validation errors:\n{0}", messages));
            throw new ValidationWarningException("Unable to save Budget:\n" + messages);
        }

        public void SavePreview(IDictionary<ApplicationDataType, object> contextObjects)
        {
            EventHandler<AdditionalInformationRequestedEventArgs> handler = Saving;
            var args = new AdditionalInformationRequestedEventArgs();
            if (handler != null)
            {
                handler(this, args);
            }

            if (args.ModificationComment.IsNothing())
            {
                args.ModificationComment = "[No comment]";
            }

            var budgetModel = args.Context as BudgetModel;
            if (budgetModel != null)
            {
                budgetModel.LastModifiedComment = args.ModificationComment;
            }
        }

        public void UpdateIncomesAndExpenses(
            [NotNull] BudgetModel model,
            IEnumerable<Income> allIncomes,
            IEnumerable<Expense> allExpenses)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            // Copy view model bound data back into model.
            model.Update(allIncomes, allExpenses);
        }

        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            var args = new ValidatingEventArgs();
            if (handler != null)
            {
                handler(this, args);
            }

            return Budgets.Validate(messages);
        }

        private void CreateNewBudgetCollection()
        {
            Budgets = this.budgetRepository.CreateNew();
        }

        private void EnsureAllBucketsUsedAreInBucketRepo()
        {
            // Make sure all buckets are in the bucket repo.
            IEnumerable<BudgetBucket> buckets = Budgets.SelectMany(b => b.Expenses.Select(e => e.Bucket))
                .Union(Budgets.SelectMany(b => b.Incomes.Select(i => i.Bucket)))
                .Distinct();

            foreach (BudgetBucket budgetBucket in buckets)
            {
                BudgetBucket copyOfBucket = budgetBucket;
                BudgetBucketRepository.GetOrCreateNew(copyOfBucket.Code, () => copyOfBucket);
            }
        }

        private static IEnumerable<Expense> CloneBudgetExpenses(BudgetModel source)
        {
            return source.Expenses.Select(
                sourceExpense => new Expense
                {
                    Amount = sourceExpense.Amount,
                    Bucket = sourceExpense.Bucket
                }).ToList();
        }

        private static IEnumerable<Income> CloneBudgetIncomes(BudgetModel source)
        {
            return source.Incomes.Select(
                sourceExpense => new Income
                {
                    Amount = sourceExpense.Amount,
                    Bucket = sourceExpense.Bucket
                }).ToList();
        }
    }
}