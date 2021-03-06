﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionToDtoAutoMapperTest
    {
        private BudgetCollectionDto Result { get; set; }

        private BudgetCollection TestData { get; set; }

        private IEnumerable<BudgetBucket> TestDataBuckets { get; set; }

        [TestMethod]
        public void ShouldMapCorrectNumberOfBuckets()
        {
            Console.WriteLine("TestDataBuckets.Count = " + TestDataBuckets.Count());
            Console.WriteLine("Result.Buckets.Count = " + Result.Buckets.Count());

            Assert.IsTrue(Result.Buckets.Any());
            foreach (var bucket in TestDataBuckets)
            {
                Assert.IsTrue(Result.Buckets.Any(b => b.Code == bucket.Code));
            }
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(TestData.FileName, Result.FileName);
        }

        [TestMethod]
        public void ShouldMapSameNumberOfBudgets()
        {
            Assert.AreEqual(TestData.Count, Result.Budgets.Count);
        }

        [TestMethod]
        public void ShouldMapBudgetsCorrectly()
        {
            Assert.AreEqual(TestData.Sum(b => b.Expenses.Sum(e => e.Amount)), Result.Budgets.Sum(b => b.Expenses.Sum(e => e.Amount)));
        }

        [TestMethod]
        public void OutputBudgetCollectionDtoResult()
        {
            decimal expensesTotal = 0;
            Console.WriteLine("TestData (BudgetCollection)");
            foreach (var budget in TestData)
            {
                Console.WriteLine("Budget: '{0}' Effective From: {1}", budget.Name, budget.EffectiveFrom);
                foreach (var expense in budget.Expenses)
                {
                    Console.WriteLine("    Expense: {0} {1}", expense.Bucket.Code, expense.Amount);
                    expensesTotal += expense.Amount;
                }
            }
            Console.WriteLine("Expenses Total: " + expensesTotal);
            Console.WriteLine();

            expensesTotal = 0;
            Console.WriteLine("Result (BudgetCollectionDto)");
            foreach (var budget in Result.Budgets)
            {
                Console.WriteLine("Budget: '{0}' Effective From: {1}", budget.Name, budget.EffectiveFrom);
                foreach (var expense in budget.Expenses)
                {
                    Console.WriteLine("    Expense: {0} {1}", expense.BudgetBucketCode, expense.Amount);
                    expensesTotal += expense.Amount;
                }
            }

            Console.WriteLine("Expenses Total: " + expensesTotal);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var config = Global.AutoMapperConfiguration;
            var budgetMapperConfig = config.Registrations.OfType<BudgetAutoMapperConfiguration>().Single();
            var bucketRepo = budgetMapperConfig.BucketRepository;

            TestData = BudgetModelTestData.CreateCollectionWith1And2();

            // ExtractXaml All Buckets from The Test Data.
            TestDataBuckets = TestData.SelectMany(b => b.Expenses.Cast<BudgetItem>())
                .Union(TestData.SelectMany(b => b.Incomes))
                .Select(x => x.Bucket)
                .Distinct();

            // Preload the buckets into the bucket repo used by the Mapper.
            foreach (var bucket in TestDataBuckets)
            {
                bucketRepo.GetByCode(bucket.Code);
            }

            Result = Mapper.Map<BudgetCollectionDto>(TestData);
        }
    }
}