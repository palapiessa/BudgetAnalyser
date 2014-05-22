﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DataLedgerEntryLine
    {
        public DataLedgerEntryLine()
        {
            Entries = new List<DataLedgerEntry>();
            BankBalanceAdjustments = new List<DataLedgerTransaction>();
            BankBalances = new List<DataBankBalance>();
        }

        /// <summary>
        /// Total bank balance, ie sum of all <see cref="BankBalances"/>
        /// </summary>
        public decimal BankBalance { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataBankBalance> BankBalances { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerTransaction> BankBalanceAdjustments { get; set; }

        public DateTime Date { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerEntry> Entries { get; set; }

        public string Remarks { get; set; }
    }
}