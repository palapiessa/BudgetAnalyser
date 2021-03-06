﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching.Data;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskMatchingRuleRepository : IMatchingRuleRepository
    {
        private readonly BasicMapper<MatchingRuleDto, MatchingRule> dataToDomainMapper;
        private readonly BasicMapper<MatchingRule, MatchingRuleDto> domainToDataMapper;

        public XamlOnDiskMatchingRuleRepository([NotNull] BasicMapper<MatchingRuleDto, MatchingRule> dataToDomainMapper, [NotNull] BasicMapper<MatchingRule, MatchingRuleDto> domainToDataMapper)
        {
            if (dataToDomainMapper == null)
            {
                throw new ArgumentNullException("dataToDomainMapper");
            }

            if (domainToDataMapper == null)
            {
                throw new ArgumentNullException("domainToDataMapper");
            }

            this.dataToDomainMapper = dataToDomainMapper;
            this.domainToDataMapper = domainToDataMapper;
        }

        public IEnumerable<MatchingRule> CreateNew()
        {
            return new List<MatchingRule>();
        }

        public async Task CreateNewAndSaveAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException("storageKey");
            }

            var newRules = new List<MatchingRule>();
            await SaveAsync(newRules, storageKey);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MatchingRuleDto")]
        public async Task<IEnumerable<MatchingRule>> LoadAsync([NotNull] string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new KeyNotFoundException("storageKey is blank");
            }

            if (!Exists(storageKey))
            {
                throw new KeyNotFoundException("Storage key can not be found: " + storageKey);
            }

            List<MatchingRuleDto> dataEntities;
            try
            {
                dataEntities = await LoadFromDiskAsync(storageKey);
            }
            catch (Exception ex)
            {
                throw new DataFormatException("Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            if (dataEntities == null)
            {
                throw new DataFormatException("Deserialised Matching-Rules are not of type List<MatchingRuleDto>");
            }

            return dataEntities.Select(d => this.dataToDomainMapper.Map(d));
        }

        public async Task SaveAsync([NotNull] IEnumerable<MatchingRule> rules, [NotNull] string storageKey)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            if (storageKey == null)
            {
                throw new ArgumentNullException("storageKey");
            }

            IEnumerable<MatchingRuleDto> dataEntities = rules.Select(r => this.domainToDataMapper.Map(r));
            await SaveToDiskAsync(storageKey, dataEntities);
        }

        protected virtual bool Exists(string storageKey)
        {
            return File.Exists(storageKey);
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for persistence - this is the type of the rehydrated object")]
        protected virtual async Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Parse(LoadXamlFromDisk(fileName)));
            return result as List<MatchingRuleDto>;
        }

        protected virtual string LoadXamlFromDisk(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        protected virtual async Task SaveToDiskAsync(string fileName, IEnumerable<MatchingRuleDto> dataEntities)
        {
            await Task.Run(() => XamlServices.Save(fileName, dataEntities.ToList()));
        }
    }
}