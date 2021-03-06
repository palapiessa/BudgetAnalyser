﻿using System;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BankImportUtilitiesTestHarness : BankImportUtilities
    {
        public BankImportUtilitiesTestHarness() : base(new FakeLogger())
        {
        }

        public Action<string> AbortIfFileDoesntExistOverride { get; set; }

        internal override void AbortIfFileDoesntExist(string fileName)
        {
            if (AbortIfFileDoesntExistOverride == null)
            {
                return;
            }

            AbortIfFileDoesntExistOverride(fileName);
        }
    }
}