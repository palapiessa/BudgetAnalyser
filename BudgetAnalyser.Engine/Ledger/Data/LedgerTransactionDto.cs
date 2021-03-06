﻿using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     A Dto for all subclasses of <see cref="LedgerTransaction" />.  All subclasses are flattened into this type.
    /// </summary>
    public class LedgerTransactionDto
    {
        public string AccountType { get; set; }
        public decimal Amount { get; set; }

        [UsedImplicitly]
        public string AutoMatchingReference { get; set; }

        [UsedImplicitly]
        public DateTime? Date { get; set; }

        public Guid Id { get; set; }
        public string Narrative { get; set; }
        public string TransactionType { get; set; }
    }
}