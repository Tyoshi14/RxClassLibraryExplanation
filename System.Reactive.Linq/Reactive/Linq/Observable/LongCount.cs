// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Observable.LongCount() 底层实现。分种情况计数观察序列元素的个数，计数所有元素个数，计数符合 predicate  条件元素个数。
    /// </summary>
    /// 由代码的流程可以看出，计数结果只有在观察序列遍历完成之后才能返回。
    /// 思考在分之二里，是不是可以通过一个平移窗口，分时段返回符合predicate 条件的元素个数。
    /// <typeparam name="TSource"></typeparam>
    class LongCount<TSource> : Producer<long>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, bool> _predicate;

        public LongCount(IObservable<TSource> source)
        {
            _source = source;
        }

        public LongCount(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        protected override IDisposable Run(IObserver<long> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            if (_predicate == null)
            {
                var sink = new _(observer, cancel);
                setSink(sink);
                return _source.SubscribeSafe(sink);
            }
            else
            {
                var sink = new LongCountImpl(this, observer, cancel);
                setSink(sink);
                return _source.SubscribeSafe(sink);
            }
        }

        class _ : Sink<long>, IObserver<TSource>
        {
            private long _count;

            public _(IObserver<long> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _count = 0L;
            }

            public void OnNext(TSource value)
            {
                try
                {
                    checked
                    {
                        _count++;
                    }
                }
                catch (Exception ex)
                {
                    base._observer.OnError(ex);
                    base.Dispose();
                }
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                base._observer.OnNext(_count);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }

        class LongCountImpl : Sink<long>, IObserver<TSource>
        {
            private readonly LongCount<TSource> _parent;
            private long _count;

            public LongCountImpl(LongCount<TSource> parent, IObserver<long> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
                _count = 0L;
            }

            public void OnNext(TSource value)
            {
                try
                {
                    checked
                    {
                        if (_parent._predicate(value))
                            _count++;
                    }
                }
                catch (Exception ex)
                {
                    base._observer.OnError(ex);
                    base.Dispose();
                }
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                base._observer.OnNext(_count);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif