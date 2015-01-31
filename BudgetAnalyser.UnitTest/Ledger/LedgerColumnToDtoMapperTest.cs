﻿using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerColumnToDtoMapperTest
    {
        private LedgerBucketDto Result { get; set; }

        private LedgerBucket TestData
        {
            get
            {
                return new LedgerBucket
                {
                    BudgetBucket = StatementModelTestData.RegoBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount,
                };
            }
        }

        [TestMethod]
        public void ShouldMapBudgetBucketCode()
        {
            Assert.AreEqual(TestDataConstants.RegoBucketCode, Result.BucketCode);
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.StoredInAccount);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerBucketDto>(TestData);
        }
    }
}