﻿using System;
using System.Globalization;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A widget to counter and test the scheduled updates that should occur at defined regular time intervals.
    /// </summary>
    public class TimedUpdateCounterWidget : Widget
    {
        private int refreshCount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimedUpdateCounterWidget" /> class.
        /// </summary>
        public TimedUpdateCounterWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[]
            {
                typeof(StatementModel), typeof(BudgetCollection), typeof(IBudgetCurrencyContext), typeof(LedgerBook), typeof(IBudgetBucketRepository), typeof(GlobalFilterCriteria),
                typeof(LedgerCalculation)
            };
            RecommendedTimeIntervalUpdate = TimeSpan.FromSeconds(10);
            DetailedText = "Refresh Count";
            Name = "Refresh Interval Counter";
            ToolTip = "Counts the number of refreshes recieved by any widget as well as refreshing every 10 seconds.";
        }

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            this.refreshCount++;
            Enabled = true;
            LargeNumber = this.refreshCount.ToString(CultureInfo.CurrentCulture);
        }
    }
}