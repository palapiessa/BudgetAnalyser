using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ChooseBudgetBucketController : ControllerBase, IShellDialogInteractivity, IShellDialogToolTips
    {
        private Guid dialogCorrelationId;
        private IEnumerable<BudgetBucket> doNotUseBudgetBuckets;
        private string doNotUseFilterDescription;
        private bool filtered;
        private readonly IAccountTypeRepository accountRepo;
        private readonly IBudgetBucketRepository bucketRepository;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public ChooseBudgetBucketController([NotNull] IUiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepository, [NotNull] IAccountTypeRepository accountRepo)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (accountRepo == null)
            {
                throw new ArgumentNullException("accountRepo");
            }

            this.bucketRepository = bucketRepository;
            this.accountRepo = accountRepo;
            BudgetBuckets = bucketRepository.Buckets.ToList();

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public IEnumerable<AccountType> BankAccounts
        {
            get { return this.accountRepo.ListCurrentlyUsedAccountTypes(); }
        }

        public IEnumerable<BudgetBucket> BudgetBuckets
        {
            get { return this.doNotUseBudgetBuckets; }

            private set
            {
                this.doNotUseBudgetBuckets = value;
                RaisePropertyChanged();
            }
        }

        public string FilterDescription
        {
            get { return this.doNotUseFilterDescription; }
            set
            {
                this.doNotUseFilterDescription = value;
                RaisePropertyChanged();
            }
        }

        public BudgetBucket Selected { get; set; }
        public bool ShowBankAccount { get; set; }
        public AccountType StoreInThisAccount { get; set; }

        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        public bool CanExecuteOkButton
        {
            get { return Selected != null; }
        }

        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public string ActionButtonToolTip
        {
            get { return "Select and use this Expense Budget Bucket."; }
        }

        public string CloseButtonToolTip
        {
            get { return "Cancel"; }
        }

        public event EventHandler<BudgetBucketChosenEventArgs> Chosen;

        public void Filter(Func<BudgetBucket, bool> predicate, string filterDescription)
        {
            FilterDescription = filterDescription;
            BudgetBuckets = this.bucketRepository.Buckets.Where(predicate).ToList();
            this.filtered = true;
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            var handler = Chosen;
            if (handler != null)
            {
                if (message.Response == ShellDialogButton.Cancel)
                {
                    handler(this, new BudgetBucketChosenEventArgs(this.dialogCorrelationId, true));
                }
                else
                {
                    handler(this, new BudgetBucketChosenEventArgs(this.dialogCorrelationId, Selected, StoreInThisAccount));
                }
            }

            Reset();
        }

        private void Reset()
        {
            if (this.filtered)
            {
                BudgetBuckets = this.bucketRepository.Buckets.ToList();
            }

            Selected = null;
            StoreInThisAccount = null;
        }

        public void ShowDialog(BudgetAnalyserFeature source, string title, Guid? correlationId = null, bool showBankAccountSelector = false)
        {
            if (correlationId == null)
            {
                this.dialogCorrelationId = Guid.NewGuid();
            }
            else
            {
                this.dialogCorrelationId = correlationId.Value;
            }

            ShowBankAccount = showBankAccountSelector;

            var dialogRequest = new ShellDialogRequestMessage(source, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = title
            };
            MessengerInstance.Send(dialogRequest);
        }
    }
}