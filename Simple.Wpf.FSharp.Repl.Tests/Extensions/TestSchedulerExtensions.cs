using System;
using Microsoft.Reactive.Testing;

namespace Simple.Wpf.FSharp.Repl.Tests.Extensions
{
    public static class TestSchedulerExtensions
    {
        public static void AdvanceBy(this TestScheduler testScheduler, TimeSpan timeSpan)
        {
            testScheduler.AdvanceBy(timeSpan.Ticks);
        }
    }
}