// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Base implement class of the Observable Empty branch. It can generate an empty Observable sequence.
    /// Considering that an empty sequence doesn't need to subscribe. That's also the reason we can't see IObservable.Subscribe here.
    /// </summary>
    class Empty<TResult> : Producer<TResult>
    {
        // Interface IScheduler represents an object that schedules units of work.
        private readonly IScheduler _scheduler;

        public Empty(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        protected override IDisposable Run(IObserver<TResult> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            // We call inner function Run() defined in class _ to return an Observable object.
            return sink.Run();
        }  

        class _ : Sink<TResult>
        {
            private readonly Empty<TResult> _parent;

            public _(Empty<TResult> parent, IObserver<TResult> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }
/// <summary>
/// An empty Observable sequence doesn't have an object,so it's unnecessary to call OnNext and OnError. The only thing we need is to call
/// OnCompleted to finish the function and return an disposable object.
/// </summary>
            public IDisposable Run()
            {
                return _parent._scheduler.Schedule(Invoke);
            }

            private void Invoke()
            {
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif