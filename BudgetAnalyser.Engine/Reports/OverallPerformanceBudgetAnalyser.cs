﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    public class OverallPerformanceBudgetAnalyser
    {
        private readonly IBudgetBucketRepository bucketRepository;

        public OverallPerformanceBudgetAnalyser([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.bucketRepository = bucketRepository;
        }

        /// <summary>
        ///     Analyses the supplied statement using the supplied budget within the criteria given to this method.
        /// </summary>
        /// <param name="budgets">The current budgets collection.</param>
        /// <param name="criteria">The criteria to limit the analysis.</param>
        /// <param name="statementModel">The current statement model.</param>
        /// <exception cref="BudgetException">
        ///     Will be thrown if no budget is supplied or if no budget can be found for the dates
        ///     given in the criteria.
        /// </exception>
        /// <exception cref="ArgumentException">If statement or budget is null.</exception>
        public virtual OverallPerformanceBudgetResult Analyse(StatementModel statementModel, BudgetCollection budgets, [NotNull] GlobalFilterCriteria criteria)
        {
            DateTime endDate, beginDate;
            AnalysisPreconditions(criteria, statementModel, budgets, out beginDate, out endDate);

            var result = new OverallPerformanceBudgetResult();

            List<BudgetModel> budgetsInvolved = budgets.ForDates(beginDate, endDate).ToList();
            result.UsesMultipleBudgets = budgetsInvolved.Count() > 1;
            BudgetModel currentBudget = budgetsInvolved.Last(); // Use most recent budget as the current

            result.DurationInMonths = StatementModel.CalculateDuration(criteria, statementModel.Transactions);

            CalculateTotalsAndAverage(beginDate, statementModel, budgets, result);

            result.AnalysesList = new List<BucketPerformanceResult>();
            var list = new List<BucketPerformanceResult>();
            foreach (BudgetBucket bucket in this.bucketRepository.Buckets)
            {
                BudgetBucket bucketCopy = bucket;
                List<Transaction> query = statementModel.Transactions.Where(t => t.BudgetBucket == bucketCopy).ToList();
                decimal totalSpent = query.Sum(t => t.Amount);
                decimal averageSpend = totalSpent / result.DurationInMonths;

                if (bucket == this.bucketRepository.SurplusBucket)
                {
                    decimal budgetedTotal = CalculateBudgetedTotalAmount(beginDate, b => b.Surplus, budgets, result);
                    decimal perMonthBudget = budgetedTotal / result.DurationInMonths; // Calc an average in case multiple budgets are used and the budgeted amounts are different.
                    var surplusAnalysis = new BucketPerformanceResult
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = budgetedTotal - totalSpent,
                        BudgetTotal = budgetedTotal,
                        Budget = perMonthBudget,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage = string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}", perMonthBudget, -averageSpend)
                    };
                    list.Add(surplusAnalysis);
                    continue;
                }

                // If the most recent budget does not contain this bucket, then skip it.
                if (currentBudget.Expenses.Any(e => e.Bucket == bucket))
                {
                    decimal totalBudget = CalculateBudgetedTotalAmount(beginDate, BuildExpenseFinder(bucket), budgets, result);
                    decimal perMonthBudget = totalBudget / result.DurationInMonths;
                    var analysis = new BucketPerformanceResult
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = perMonthBudget,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage = string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}", perMonthBudget, -averageSpend)
                    };
                    list.Add(analysis);
                    continue;
                }

                // If the most recent budget does not contain this bucket, then skip it.
                if (currentBudget.Incomes.Any(i => i.Bucket == bucket))
                {
                    decimal totalBudget = CalculateBudgetedTotalAmount(beginDate, BuildIncomeFinder(bucket), budgets, result);
                    decimal perMonthBudget = totalBudget / result.DurationInMonths;
                    var analysis = new BucketPerformanceResult
                    {
                        Bucket = bucket,
                        TotalSpent = totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = perMonthBudget,
                        AverageSpend = averageSpend,
                        BudgetComparedToAverage = string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per month: {1:C}", perMonthBudget, -averageSpend)
                    };
                    list.Add(analysis);
                }
            }

            result.AnalysesList = list.OrderByDescending(a => a.Percent).ToList();
            return result;
        }

        private static Func<BudgetModel, decimal> BuildExpenseFinder(BudgetBucket bucket)
        {
            return b =>
            {
                Expense first = b.Expenses.FirstOrDefault(e => e.Bucket == bucket);
                if (first == null)
                {
                    return 0;
                }

                return first.Amount;
            };
        }

        private static Func<BudgetModel, decimal> BuildIncomeFinder(BudgetBucket bucket)
        {
            return b =>
            {
                Income first = b.Incomes.FirstOrDefault(e => e.Bucket == bucket);
                if (first == null)
                {
                    return 0;
                }

                return first.Amount;
            };
        }

        private static void AnalysisPreconditions(GlobalFilterCriteria criteria, StatementModel statement, BudgetCollection budgets, out DateTime beginDate, out DateTime endDate)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if (!criteria.Cleared && (criteria.BeginDate == null || criteria.EndDate == null))
            {
                throw new ArgumentException("The given criteria does not contain any filtering dates.");
            }

            if (statement == null)
            {
                throw new ArgumentNullException("statement", "The statement supplied is null, analysis cannot proceed with no statement.");
            }

            if (budgets == null)
            {
                throw new ArgumentNullException("budgets");
            }

            if (criteria.Cleared)
            {
                beginDate = statement.AllTransactions.First().Date;
                endDate = statement.AllTransactions.Last().Date;
            }
            else
            {
                beginDate = criteria.BeginDate.Value;
                endDate = criteria.EndDate.Value;
            }
        }

        private static decimal CalculateBudgetedTotalAmount(DateTime beginDate, Func<BudgetModel, decimal> whichBudgetBucket, BudgetCollection budgets, OverallPerformanceBudgetResult result)
        {
            if (!result.UsesMultipleBudgets)
            {
                return whichBudgetBucket(budgets.ForDate(beginDate)) * result.DurationInMonths;
            }

            decimal budgetedAmount = 0;
            for (int month = 0; month < result.DurationInMonths; month++)
            {
                BudgetModel budget = budgets.ForDate(beginDate.AddMonths(month));
                budgetedAmount += whichBudgetBucket(budget);
            }

            return budgetedAmount;
        }

        private static void CalculateTotalsAndAverage(DateTime beginDate, StatementModel statement, BudgetCollection budgets, OverallPerformanceBudgetResult result)
        {
            // First total the expenses without the saved up for expenses.
            decimal totalExpensesSpend = statement.Transactions
                .Where(t => t.BudgetBucket is ExpenseBucket)
                .Sum(t => t.Amount);

            decimal totalSurplusSpend = statement.Transactions
                .Where(t => t.BudgetBucket is SurplusBucket)
                .Sum(t => t.Amount);

            result.AverageSpend = totalExpensesSpend / result.DurationInMonths; // Expected to be negative
            result.AverageSurplus = totalSurplusSpend / result.DurationInMonths; // Expected to be negative

            for (int month = 0; month < result.DurationInMonths; month++)
            {
                BudgetModel budget = budgets.ForDate(beginDate.AddMonths(month));
                result.TotalBudgetExpenses += budget.Expenses.Sum(e => e.Amount);
            }

            result.OverallPerformance = result.AverageSpend + result.TotalBudgetExpenses;
        }
    }
}