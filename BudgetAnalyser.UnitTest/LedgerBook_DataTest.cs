﻿using System;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    // ReSharper disable InconsistentNaming

    [TestClass]
    public class LedgerBook_DataTest
    {
        [TestMethod]
        public void UsingTestData1_FirstLineHasNoAdjustments()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(0, result.DatedEntries.First().BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void UsingTestData1_FirstLineBankBalanceEqualsLedgerBalance()
        {
            var result = ArrangeAndAct();
            var subject = result.DatedEntries.First();
            Assert.AreEqual(subject.LedgerBalance, subject.TotalBankBalance);
        }

        [TestMethod]
        public void UsingTestData1_LedgerCountShouldBe3()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(3, result.Ledgers.Count());
        }

        [TestMethod]
        public void UsingTestData1_ShouldBeHairLedger()
        {
            var result = ArrangeAndAct();
            Assert.IsNotNull(result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode));
        }

        [TestMethod]
        public void UsingTestData1_ShouldBePhoneLedger()
        {
            var result = ArrangeAndAct();
            Assert.IsNotNull(result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.PhoneBucketCode));
        }

        [TestMethod]
        public void UsingTestData1_ShouldBePowerLedger()
        {
            var result = ArrangeAndAct();
            Assert.IsNotNull(result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.PowerBucketCode));
        }

        [TestMethod]
        public void UsingTestData1_ShouldHave3Lines()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(3, result.DatedEntries.Count());
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHaveBankBalance2950()
        {
            var result = ArrangeAndAct();
            var subject = result.DatedEntries.First();
            Assert.AreEqual(2950, subject.TotalBankBalance);
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHaveDate20130815()
        {
            var result = ArrangeAndAct();
            var subject = result.DatedEntries.First();
            Assert.AreEqual(new DateTime(2013, 08, 15), subject.Date);
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHaveSurplusOf2712()
        {
            var result = ArrangeAndAct();
            var subject = result.DatedEntries.First();
            Assert.AreEqual(2712.97M, subject.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHave3Entries()
        {
            var result = ArrangeAndAct();
            var subject = result.DatedEntries.First();
            Assert.AreEqual(3, subject.Entries.Count());
        }

        [TestMethod]
        public void UsingTestData1_FirstLineHairEntryShouldHaveBalance120()
        {
            var result = ArrangeAndAct();
            var subject = result.DatedEntries.First().Entries.First(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            Assert.AreEqual(120, subject.Balance);
        }

        [TestMethod]
        public void UsingTestData1_OutputDataInTabularForm()
        {
            var result = ArrangeAndAct();
            result.Output();
        }

        private LedgerBook ArrangeAndAct()
        {
            return TestData.LedgerBookTestData.TestData1();
        }
    }
    // ReSharper restore InconsistentNaming
}
