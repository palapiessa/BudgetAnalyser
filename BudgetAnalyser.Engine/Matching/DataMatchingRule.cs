﻿using System;

namespace BudgetAnalyser.Engine.Matching
{
    public class DataMatchingRule
    {
        public decimal? Amount { get; set; }

        public string BucketCode { get; set; }

        public string Description { get; set; }

        public DateTime? LastMatch { get; set; }

        public int MatchCount { get; set; }

        public string Reference1 { get; set; }

        public string Reference2 { get; set; }

        public string Reference3 { get; set; }

        public string TransactionType { get; set; }
    }
}