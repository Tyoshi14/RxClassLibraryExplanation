// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{   
    /// <summary>
    /// Observable.IsEmpty() 的底层实现，用于判断观察序列是否为空。
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    class IsEmpty<TSource> : Producer<bool>
    {
        private readonly IObservable<TSource> _source;

        public IsEmpty(IObservable<TSource> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<bool> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<bool>, IObserver<TSource>
        {
            public _(IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }

            public void OnNext(TSource value)
            {
                base._observer.OnNext(false);
                base._observer.OnCompleted();
                base.Dispose();
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                base._observer.OnNext(true);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif