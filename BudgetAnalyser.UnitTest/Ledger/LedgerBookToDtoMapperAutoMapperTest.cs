﻿using System;
using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookToDtoMapperAutoMapperTest
    {
        private LedgerBookDto Result { get; set; }

        private LedgerBook TestData
        {
            get { return LedgerBookTestData.TestData4(); }
        }

        [TestMethod]
        public void ShouldMapReconciliations()
        {
            Assert.AreEqual(3, Result.Reconciliations.Count);
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual("C:\\Folder\\book1.xml", Result.FileName);
        }

        [TestMethod]
        public void ShouldMapModified()
        {
            Assert.AreEqual(new DateTime(2013,12,16), Result.Modified);
        }

        [TestMethod]
        public void ShouldMapName()
        {
            Assert.AreEqual("Test Data 4 Book", Result.Name);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            

            Result = Mapper.Map<LedgerBookDto>(TestData);
        }
    }
}