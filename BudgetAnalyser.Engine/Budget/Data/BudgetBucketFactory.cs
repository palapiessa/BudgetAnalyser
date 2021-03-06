using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class BudgetBucketFactory : IBudgetBucketFactory
    {
        public BudgetBucket Build(BucketDtoType type)
        {
            switch (type)
            {
                case BucketDtoType.Income:
                    return new IncomeBudgetBucket();
                case BucketDtoType.Journal:
                case BucketDtoType.Surplus:
                    throw new NotSupportedException("You may not create multiple instances of the Journal or Surplus buckets.");
                case BucketDtoType.SavedUpForExpense:
                    return new SavedUpForExpenseBucket();
                case BucketDtoType.SavingsCommitment:
                    return new SavingsCommitmentBucket();
                case BucketDtoType.SpentMonthlyExpense:
                    return new SpentMonthlyExpenseBucket();
                case BucketDtoType.FixedBudgetProject:
                    return new FixedBudgetProjectBucket();
                default:
                    throw new NotSupportedException("Unsupported Bucket type detected: " + type);
            }
        }

        public BucketDtoType SerialiseType([NotNull] BudgetBucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            if (bucket is IncomeBudgetBucket)
            {
                return BucketDtoType.Income;
            }

            if (bucket is FixedBudgetProjectBucket)
            {
                throw new NotSupportedException();
            }

            if (bucket is SurplusBucket)
            {
                return BucketDtoType.Surplus;
            }

            if (bucket is JournalBucket)
            {
                return BucketDtoType.Journal;
            }

            if (bucket is SavedUpForExpenseBucket)
            {
                return BucketDtoType.SavedUpForExpense;
            }

            if (bucket is SpentMonthlyExpenseBucket)
            {
                return BucketDtoType.SpentMonthlyExpense;
            }

            if (bucket is SavingsCommitmentBucket)
            {
                return BucketDtoType.SavingsCommitment;
            }

            throw new NotSupportedException("Unsupported bucket type detected: " + bucket.GetType().FullName);
        }
    }
}