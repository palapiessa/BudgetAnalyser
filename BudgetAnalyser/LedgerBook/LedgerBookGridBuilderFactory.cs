﻿using System.Windows.Input;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
    public class LedgerBookGridBuilderFactory
    {
        public virtual ILedgerBookGridBuilder GridBuilderV2(
            ICommand showTransactionsCommand,
            ICommand showBankBalancesCommand,
            ICommand showRemarksCommand,
            ICommand removeLedgerEntryLineCommand,
            ICommand showHideMonthsCommand,
            ICommand showSurplusBalancesCommand)
        {
            return new LedgerBookGridBuilderV2(showTransactionsCommand, showBankBalancesCommand, showRemarksCommand, removeLedgerEntryLineCommand, showHideMonthsCommand, showSurplusBalancesCommand);
        }
    }
}
