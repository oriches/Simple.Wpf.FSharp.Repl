﻿namespace Simple.Wpf.FSharp.Repl.Tests.Extensions
{
    using System;
    using Microsoft.Reactive.Testing;

    public static class TestSchedulerExtensions
    {
        public static void AdvanceBy(this TestScheduler testScheduler, TimeSpan timeSpan)
        {
            testScheduler.AdvanceBy(timeSpan.Ticks);
        }
    }
}
