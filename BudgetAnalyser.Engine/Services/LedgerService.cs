using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerService : ILedgerService, ISupportsModelPersistence
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly ILedgerBookRepository ledgerRepository;
        private readonly ILogger logger;

        public LedgerService(
            [NotNull] ILedgerBookRepository ledgerRepository,
            [NotNull] IAccountTypeRepository accountTypeRepository,
            [NotNull] ILogger logger)
        {
            if (ledgerRepository == null)
            {
                throw new ArgumentNullException("ledgerRepository");
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.ledgerRepository = ledgerRepository;
            this.accountTypeRepository = accountTypeRepository;
            this.logger = logger;
        }

        public event EventHandler Closed;
        public event EventHandler NewDataSourceAvailable;
        public event EventHandler Saved;
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;
        public event EventHandler<ValidatingEventArgs> Validating;

        public ApplicationDataType DataType
        {
            get { return ApplicationDataType.Ledger; }
        }

        public LedgerBook LedgerBook { get; private set; }

        public int LoadSequence
        {
            get { return 50; }
        }

        public ToDoCollection ReconciliationToDoList { get; private set; }

        public void CancelBalanceAdjustment(LedgerEntryLine entryLine, Guid transactionId)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException("entryLine");
            }

            if (LedgerBook.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", "entryLine");
            }

            entryLine.CancelBalanceAdjustment(transactionId);
        }

        public void Close()
        {
            LedgerBook = null;
            ReconciliationToDoList = new ToDoCollection();
            EventHandler handler = Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public async Task CreateAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase.LedgerBookStorageKey.IsNothing())
            {
                throw new ArgumentNullException("applicationDatabase");
            }

            await this.ledgerRepository.CreateNewAndSaveAsync(applicationDatabase.LedgerBookStorageKey);
            await LoadAsync(applicationDatabase);
        }

        public LedgerTransaction CreateBalanceAdjustment(LedgerEntryLine entryLine, decimal amount, string narrative, AccountType account)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException("entryLine");
            }

            if (narrative == null)
            {
                throw new ArgumentNullException("narrative");
            }

            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (LedgerBook.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", "entryLine");
            }

            BankBalanceAdjustmentTransaction adjustmentTransaction = entryLine.BalanceAdjustment(amount, narrative).WithAccountType(account);
            adjustmentTransaction.Date = entryLine.Date;
            return adjustmentTransaction;
        }

        public LedgerTransaction CreateLedgerTransaction(LedgerEntry ledgerEntry, decimal amount, string narrative)
        {
            if (ledgerEntry == null)
            {
                throw new ArgumentNullException("ledgerEntry");
            }

            if (narrative == null)
            {
                throw new ArgumentNullException("narrative");
            }

            if (LedgerBook.Reconciliations.First().Entries.All(e => e != ledgerEntry))
            {
                throw new ArgumentException("Ledger Entry provided does not exist in the current Ledger Book.", "ledgerEntry");
            }

            LedgerTransaction newTransaction = new CreditLedgerTransaction();
            newTransaction.WithAmount(amount).WithNarrative(narrative);
            newTransaction.Date = LedgerBook.Reconciliations.First().Date;
            ledgerEntry.AddTransaction(newTransaction);
            return newTransaction;
        }

        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase == null)
            {
                throw new ArgumentNullException("applicationDatabase");
            }

            ReconciliationToDoList = applicationDatabase.LedgerReconciliationToDoCollection;
            LedgerBook = await this.ledgerRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.LedgerBookStorageKey));

            EventHandler handler = NewDataSourceAvailable;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public LedgerEntryLine MonthEndReconciliation(
            DateTime reconciliationDateIfFirstEver,
            IEnumerable<BankBalance> balances,
            IBudgetCurrencyContext budgetContext,
            StatementModel statement,
            bool ignoreWarnings = false)
        {
            if (balances == null)
            {
                throw new ArgumentNullException("balances");
            }

            if (budgetContext == null)
            {
                throw new ArgumentNullException("budgetContext");
            }

            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            if (!budgetContext.BudgetActive)
            {
                throw new InvalidOperationException("Reconciling against an inactive budget is invalid.");
            }

            ReconciliationToDoList.Clear();
            Stopwatch stopWatch = Stopwatch.StartNew();
            this.logger.LogInfo(l => l.Format("Starting Ledger Book reconciliation {0}", DateTime.Now));
            LedgerEntryLine recon = LedgerBook.Reconcile(reconciliationDateIfFirstEver, balances, budgetContext.Model, ReconciliationToDoList, statement, ignoreWarnings);
            foreach (ToDoTask task in ReconciliationToDoList)
            {
                this.logger.LogInfo(l => l.Format("TASK: {0} SystemGenerated:{1}", task.Description, task.SystemGenerated));
            }

            stopWatch.Stop();
            this.logger.LogInfo(l => l.Format("Finished Ledger Book reconciliation {0}. It took {1:F0}ms", DateTime.Now, stopWatch.ElapsedMilliseconds));
            return recon;
        }

        public void MoveLedgerToAccount(LedgerBook ledgerBook, LedgerBucket ledger, AccountType storedInAccount)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }
            if (ledger == null)
            {
                throw new ArgumentNullException("ledger");
            }
            if (storedInAccount == null)
            {
                throw new ArgumentNullException("storedInAccount");
            }

            ledgerBook.SetLedgerAccount(ledger, storedInAccount);
        }

        public void RemoveReconciliation(LedgerEntryLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            LedgerBook.RemoveLine(line);
        }

        public void RemoveTransaction(LedgerEntry ledgerEntry, Guid transactionId)
        {
            if (ledgerEntry == null)
            {
                throw new ArgumentNullException("ledgerEntry");
            }

            if (LedgerBook.Reconciliations.First().Entries.Any(e => e == ledgerEntry))
            {
                throw new ArgumentException("Ledger Entry provided does not exist in the current Ledger Book.", "ledgerEntry");
            }

            ledgerEntry.RemoveTransaction(transactionId);
        }

        public void RenameLedgerBook(LedgerBook ledgerBook, string newName)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }
            if (newName == null)
            {
                throw new ArgumentNullException("newName");
            }

            ledgerBook.Name = newName;
        }

        public async Task SaveAsync(IDictionary<ApplicationDataType, object> contextObjects)
        {
            EventHandler<AdditionalInformationRequestedEventArgs> savingHandler = Saving;
            if (savingHandler != null)
            {
                savingHandler(this, new AdditionalInformationRequestedEventArgs());
            }

            var messages = new StringBuilder();
            if (!LedgerBook.Validate(messages))
            {
                throw new ValidationWarningException("Ledger Book is invalid, cannot save at this time:\n" + messages);
            }

            await this.ledgerRepository.SaveAsync(LedgerBook, LedgerBook.FileName);
            EventHandler handler = Saved;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void SavePreview(IDictionary<ApplicationDataType, object> contextObjects)
        {
        }

        public LedgerBucket TrackNewBudgetBucket(ExpenseBucket bucket, AccountType storeInThisAccount)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            if (storeInThisAccount == null)
            {
                throw new ArgumentNullException("storeInThisAccount");
            }

            return LedgerBook.AddLedger(bucket, storeInThisAccount);
        }

        public LedgerEntryLine UnlockCurrentMonth()
        {
            return LedgerBook.UnlockMostRecentLine();
        }

        public void UpdateRemarks(LedgerEntryLine entryLine, string remarks)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException("entryLine");
            }

            if (remarks == null)
            {
                throw new ArgumentNullException("remarks");
            }

            if (LedgerBook.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", "entryLine");
            }

            entryLine.UpdateRemarks(remarks);
        }

        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            if (handler != null)
            {
                handler(this, new ValidatingEventArgs());
            }

            return LedgerBook.Validate(messages);
        }

        public IEnumerable<AccountType> ValidLedgerAccounts()
        {
            return this.accountTypeRepository.ListCurrentlyUsedAccountTypes();
        }
    }
}