﻿using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A class to run all AutoMapper configuration in this assembly.  Any class that implements <see cref="ILocalAutoMapperConfiguration"/> will be dependency injected into this class 
    /// and all <see cref="ILocalAutoMapperConfiguration.RegisterMappings"/> methods on each are called.  The IoC container takes care of finding all the instances that implement the interface.
    /// </summary>
    public class AutoMapperConfiguration
    {
        private readonly IEnumerable<ILocalAutoMapperConfiguration> autoMapperRegistrations;

        public AutoMapperConfiguration([NotNull] IEnumerable<ILocalAutoMapperConfiguration> autoMapperRegistrations)
        {
            if (autoMapperRegistrations == null)
            {
                throw new ArgumentNullException("autoMapperRegistrations");
            }

            this.autoMapperRegistrations = autoMapperRegistrations;
        }

        /// <summary>
        /// Register all configurations on all known instances. This only needs to be done once during application startup.
        /// </summary>
        public AutoMapperConfiguration Configure()
        {
            // Warning! Use of this static mapping configuration has a chance of intermittently failing tests, if the tests are run in parallel (which they are in this project).
            // This may need to change to improve consistency of test results. Failure does seem to be very rare however.

            foreach (var configuration in autoMapperRegistrations)
            {
                configuration.RegisterMappings();
            }

            return this;
        }
    }
}