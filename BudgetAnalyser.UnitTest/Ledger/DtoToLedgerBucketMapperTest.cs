﻿using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerBucketMapperTest
    {
        private LedgerBucket Result { get; set; }

        private LedgerBucketDto TestData
        {
            get
            {
                return new LedgerBucketDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    StoredInAccount = TestDataConstants.ChequeAccountName
                };
            }
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.StoredInAccount.Name);
        }

        [TestMethod]
        public void ShouldMapBudgetBucketCode()
        {
            Assert.AreEqual(TestDataConstants.RegoBucketCode, Result.BudgetBucket.Code);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerBucket>(TestData);
        }
    }
}