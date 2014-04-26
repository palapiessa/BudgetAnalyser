using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Statement
{
    public class StatementViewModel : INotifyPropertyChanged
    {
        public const string UncategorisedFilter = "[Uncategorised Only]";
        private readonly IBudgetBucketRepository budgetBucketRepository;

        private string doNotUseBucketFilter;

        private bool doNotUseDirty;
        private string doNotUseDuplicateSummary;
        private ObservableCollection<TransactionGroupedByBucket> doNotUseGroupedByBucket;
        private bool doNotUseSortByDate;
        private StatementModel doNotUseStatement;

        public StatementViewModel([NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.budgetBucketRepository = budgetBucketRepository;
            SortByDate = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public decimal AverageDebit
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    IEnumerable<Transaction> query = Statement.Transactions.Where(t => t.Amount < 0).ToList();
                    if (query.Any())
                    {
                        return query.Average(t => t.Amount);
                    }
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    List<Transaction> query2 =
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code)).ToList();
                    if (query2.Any())
                    {
                        return query2.Average(t => t.Amount);
                    }

                    return 0;
                }

                IEnumerable<Transaction> query3 = Statement.Transactions
                    .Where(
                        t =>
                            t.Amount < 0 && t.BudgetBucket != null &&
                            t.BudgetBucket.Code == BucketFilter)
                    .ToList();
                if (query3.Any())
                {
                    return query3.Average(t => t.Amount);
                }

                return 0;
            }
        }

        public string BucketFilter
        {
            get { return this.doNotUseBucketFilter; }

            set
            {
                // TODO Change to a multi-select drop down and allow one or many buckets to be selected.
                this.doNotUseBucketFilter = value;
                OnPropertyChanged();
                TriggerRefreshTotalsRow();
            }
        }

        public IEnumerable<string> BudgetBuckets
        {
            get
            {
                return this.budgetBucketRepository.Buckets
                    .Select(b => b.Code)
                    .Union(new[] { string.Empty }).OrderBy(b => b);
            }
        }

        public BudgetModel BudgetModel { get; set; }

        public bool Dirty
        {
            get { return this.doNotUseDirty; }

            set
            {
                this.doNotUseDirty = value;
                OnPropertyChanged();
            }
        }

        public string DuplicateSummary
        {
            get { return this.doNotUseDuplicateSummary; }

            private set
            {
                this.doNotUseDuplicateSummary = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> FilterBudgetBuckets
        {
            get { return BudgetBuckets.Union(new[] { UncategorisedFilter }).OrderBy(b => b); }
        }

        public ObservableCollection<TransactionGroupedByBucket> GroupedByBucket
        {
            get { return this.doNotUseGroupedByBucket; }
            private set
            {
                this.doNotUseGroupedByBucket = value;
                OnPropertyChanged();
            }
        }

        public bool HasTransactions
        {
            get { return Statement != null && Statement.Transactions.Any(); }
        }

        public DateTime MaxTransactionDate
        {
            get { return Statement.Transactions.Max(t => t.Date); }
        }

        public DateTime MinTransactionDate
        {
            get { return Statement.Transactions.Min(t => t.Date); }
        }

        public bool SortByBucket
        {
            get { return !this.doNotUseSortByDate; }
            set
            {
                this.doNotUseSortByDate = !value;
                OnPropertyChanged("SortByDate");
                OnPropertyChanged();
            }
        }

        public bool SortByDate
        {
            get { return this.doNotUseSortByDate; }
            set
            {
                this.doNotUseSortByDate = value;
                OnPropertyChanged("SortByBucket");
                OnPropertyChanged();
            }
        }

        public StatementModel Statement
        {
            get { return this.doNotUseStatement; }

            set
            {
                if (this.doNotUseStatement != null)
                {
                    this.doNotUseStatement.PropertyChanged -= OnStatementPropertyChanged;
                }

                this.doNotUseStatement = value;
                this.doNotUseStatement.PropertyChanged += OnStatementPropertyChanged;
                OnPropertyChanged();
                UpdateGroupedByBucket();
            }
        }

        private void OnStatementPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // Caters for deleting a transaction. Could be more efficient if it becomes a problem.
            if (propertyChangedEventArgs.PropertyName == "Transactions")
            {
                UpdateGroupedByBucket();
            }
        }

        public string StatementName
        {
            get
            {
                if (Statement != null)
                {
                    return Path.GetFileNameWithoutExtension(Statement.FileName);
                }

                return "[No Transactions Loaded]";
            }
        }

        public decimal TotalCount
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Count();
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Count(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code));
                }

                return Statement.Transactions.Count(t => t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter);
            }
        }

        public decimal TotalCredits
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code))
                            .Sum(t => t.Amount);
                }

                return
                    Statement.Transactions.Where(
                        t => t.Amount > 0 && t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter)
                        .Sum(t => t.Amount);
            }
        }

        public decimal TotalDebits
        {
            get
            {
                if (Statement == null || Statement.Transactions == null)
                {
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(BucketFilter))
                {
                    return Statement.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
                }

                if (BucketFilter == UncategorisedFilter)
                {
                    return
                        Statement.Transactions.Where(
                            t => t.BudgetBucket == null || string.IsNullOrWhiteSpace(t.BudgetBucket.Code))
                            .Sum(t => t.Amount);
                }

                return
                    Statement.Transactions.Where(
                        t => t.Amount < 0 && t.BudgetBucket != null && t.BudgetBucket.Code == BucketFilter)
                        .Sum(t => t.Amount);
            }
        }

        public decimal TotalDifference
        {
            get { return TotalCredits + TotalDebits; }
        }

        public void TriggerRefreshTotalsRow()
        {
            OnPropertyChanged("TotalCredits");
            OnPropertyChanged("TotalDebits");
            OnPropertyChanged("TotalDifference");
            OnPropertyChanged("AverageDebit");
            OnPropertyChanged("TotalCount");
            OnPropertyChanged("HasTransactions");
            OnPropertyChanged("StatementName");
            OnPropertyChanged("MinTransactionDate");
            OnPropertyChanged("MaxTransactionDate");

            if (Statement == null)
            {
                DuplicateSummary = null;
            }
            else
            {
                List<IGrouping<int, Transaction>> duplicates = Statement.ValidateAgainstDuplicates().ToList();
                DuplicateSummary = duplicates.Any()
                    ? string.Format(CultureInfo.CurrentCulture, "{0} suspected duplicates!",
                        duplicates.Sum(group => group.Count()))
                    : null;
            }
        }

        public void UpdateGroupedByBucket()
        {
            if (SortByBucket)
            {
                IEnumerable<TransactionGroupedByBucket> query = Statement.Transactions
                    .GroupBy(t => t.BudgetBucket)
                    .OrderBy(g => g.Key)
                    .Select(group => new TransactionGroupedByBucket(group, group.Key));
                GroupedByBucket = new ObservableCollection<TransactionGroupedByBucket>(query);
            }
            else
            {
                // Do it later - its not shown right now.
                GroupedByBucket = new ObservableCollection<TransactionGroupedByBucket>();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}