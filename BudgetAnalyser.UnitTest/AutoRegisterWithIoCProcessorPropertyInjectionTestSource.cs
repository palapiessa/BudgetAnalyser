﻿using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.UnitTest
{
    [AutoRegisterWithIoC]
    [UsedImplicitly]
    public static class AutoRegisterWithIoCProcessorPropertyInjectionTestSource
    {
        public static ILogger NotInjected { get; set; }

        [PropertyInjection]
        public static ILogger Logger { get; set; }
    }
}