﻿using System;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Matching;

namespace BudgetAnalyser.Statement
{
    public class TransactionGroupedByBucketViewModel : TransactionGroupedByBucket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Cannot validate before being passed to base ctor")]
        public TransactionGroupedByBucketViewModel([NotNull] TransactionGroupedByBucket baseline, [NotNull] IUiContext uiContext)
            : base(baseline.Transactions, baseline.Bucket)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            DeleteTransactionCommand = uiContext.StatementController.DeleteTransactionCommand;
            EditTransactionCommand = uiContext.StatementController.EditTransactionCommand;
            AppliedRulesController = uiContext.StatementController.AppliedRulesController;
        }

        [UsedImplicitly]
        public AppliedRulesController AppliedRulesController { get; private set; }
        [UsedImplicitly]
        public ICommand DeleteTransactionCommand { get; private set; }
        [UsedImplicitly]
        public ICommand EditTransactionCommand { get; private set; }
    }
}