// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace System.Reactive.Concurrency
{
    internal static class SchedulerDefaults
    {
        /// <summary>
        /// This function returns an ImmediateScheduler instance.
        ///   Class ImmediateScheduler represents an object that schedules units of work to run immediately on the current thread.
        /// </summary>
        internal static IScheduler ConstantTimeOperations { get { return ImmediateScheduler.Instance; } }
        internal static IScheduler TailRecursion { get { return ImmediateScheduler.Instance; } }

        /// <summary>
        /// CurrentThreadSchedular represents an object that schedules units of work on the current thread.
        ///   Iteration function return an CurrentThreadSchedular instance.
        /// </summary>
        internal static IScheduler Iteration { get { return CurrentThreadScheduler.Instance; } }
        internal static IScheduler TimeBasedOperations { get { return DefaultScheduler.Instance; } }
        internal static IScheduler AsyncConversions { get { return DefaultScheduler.Instance; } }
    }
}
