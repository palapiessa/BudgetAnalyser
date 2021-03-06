using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine.Ledger
{
    public class TransferTask : ToDoTask
    {
        public TransferTask(string description, bool systemGenerated = false, bool canDelete = true) : base(description, systemGenerated, canDelete)
        {
        }

        public decimal Amount { get; internal set; }
        public AccountType SourceAccount { get; internal set; }
        public AccountType DestinationAccount { get; internal set; }
    }
}