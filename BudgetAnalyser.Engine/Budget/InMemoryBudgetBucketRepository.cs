﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A thread-safe in memory implementation of <see cref="IBudgetBucketRepository" />.
    ///     This repository does not need to be persisted, because it uses the <see cref="BudgetCollection" /> as the source of
    ///     truth.
    ///     Thread safety is built in to allow multiple threads to load data from statements asynchronously.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryBudgetBucketRepository : IBudgetBucketRepository
    {
        private readonly BasicMapper<BudgetBucketDto, BudgetBucket> mapper;
        private readonly object syncRoot = new object();
        private Dictionary<string, BudgetBucket> lookupTable = new Dictionary<string, BudgetBucket>();

        public InMemoryBudgetBucketRepository([NotNull] BasicMapper<BudgetBucketDto, BudgetBucket> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException("mapper");
            }

            this.mapper = mapper;
        }

        public virtual IEnumerable<BudgetBucket> Buckets
        {
            get { return this.lookupTable.Values.OrderBy(b => b.Code).ToList(); }
        }

        public BudgetBucket SurplusBucket { get; protected set; }

        public FixedBudgetProjectBucket CreateNewFixedBudgetProject(string bucketCode, string description, decimal fixedBudgetAmount)
        {
            if (string.IsNullOrWhiteSpace(bucketCode))
            {
                throw new ArgumentNullException("bucketCode");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("description");
            }

            if (fixedBudgetAmount <= 0)
            {
                throw new ArgumentException("The fixed budget amount must be greater than zero.", "fixedBudgetAmount");
            }

            var upperCode = FixedBudgetProjectBucket.CreateCode(bucketCode);
            if (IsValidCode(upperCode))
            {
                throw new ArgumentException("A new fixed budget project bucket cannot be created, because the code " + bucketCode + " already exists.", bucketCode);
            }

            lock (this.syncRoot)
            {
                if (IsValidCode(upperCode))
                {
                    throw new ArgumentException("A new fixed budget project bucket cannot be created, because the code " + bucketCode + " already exists.", bucketCode);
                }

                var bucket = new FixedBudgetProjectBucket(bucketCode, description, fixedBudgetAmount);
                this.lookupTable.Add(upperCode, bucket);
                return bucket;
            }
        }

        public virtual BudgetBucket GetByCode(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            var upperCode = code.ToUpperInvariant();
            if (IsValidCode(upperCode))
            {
                return this.lookupTable[upperCode];
            }

            return null;
        }

        public BudgetBucket GetOrCreateNew(string code, Func<BudgetBucket> factory)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            var upperCode = code.ToUpperInvariant();
            if (IsValidCode(upperCode))
            {
                return this.lookupTable[upperCode];
            }

            lock (this.syncRoot)
            {
                if (IsValidCode(upperCode))
                {
                    return this.lookupTable[upperCode];
                }

                var newBucket = factory();
                this.lookupTable.Add(upperCode, newBucket);
                return newBucket;
            }
        }

        public virtual void Initialise(IEnumerable<BudgetBucketDto> buckets)
        {
            if (buckets == null)
            {
                throw new ArgumentNullException("buckets");
            }

            this.lookupTable = buckets
                .Where(dto => dto.Type != BucketDtoType.Journal && dto.Type != BucketDtoType.Surplus)
                .Select(dto => this.mapper.Map(dto))
                .Distinct()
                .ToDictionary(e => e.Code, e => e);

            InitialiseMandatorySpecialBuckets();
        }

        public virtual bool IsValidCode(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            return this.lookupTable.ContainsKey(code.ToUpperInvariant());
        }

        /// <summary>
        ///     Removes the fixed budget project bucket permanently
        /// </summary>
        /// <param name="projectBucket">The project bucket to remove.</param>
        public void RemoveFixedBudgetProject(FixedBudgetProjectBucket projectBucket)
        {
            if (projectBucket == null)
            {
                throw new ArgumentNullException("projectBucket");
            }

            lock (this.syncRoot)
            {
                if (!IsValidCode(projectBucket.Code))
                {
                    throw new ArgumentException("Unable to remove the fixed project bucket, it does not appear to exist in the bucket repository.");
                }

                if (!this.lookupTable.Remove(projectBucket.Code))
                {
                    throw new InvalidOperationException("Unable to remove the fixed project bucket, removal from the internal dictionary failed. " + projectBucket.Code);
                }
            }
        }

        protected void AddBucket([NotNull] BudgetBucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            if (IsValidCode(bucket.Code))
            {
                return;
            }

            lock (this.syncRoot)
            {
                if (IsValidCode(bucket.Code))
                {
                    return;
                }

                this.lookupTable.Add(bucket.Code, bucket);
            }
        }

        protected void InitialiseMandatorySpecialBuckets()
        {
            SurplusBucket = new SurplusBucket();
            AddBucket(SurplusBucket);
            AddBucket(new JournalBucket(JournalBucket.JournalCode, "A special bucket to allocate internal transfers."));
        }
    }
}