// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    class FirstAsync<TSource> : Producer<TSource>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, bool> _predicate;
        private readonly bool _throwOnEmpty;

        public FirstAsync(IObservable<TSource> source, Func<TSource, bool> predicate, bool throwOnEmpty)
        {
            _source = source;
            _predicate = predicate;
            _throwOnEmpty = throwOnEmpty;
        }
        // Observable.FirstAsync()功能的底层实现。功能分为两个分支，一是直接返回观察序列的第一个元素；二是，返回观察序列中符合条件predicate的第一个元素。
        protected override IDisposable Run(IObserver<TSource> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            if (_predicate != null)
            {
                var sink = new FirstAsyncImpl(this, observer, cancel);
                setSink(sink);
                return _source.SubscribeSafe(sink);
            }
            else
            {
                var sink = new _(this, observer, cancel);
                setSink(sink);
                return _source.SubscribeSafe(sink);
            }
        }

        //直接返回观察序列的第一个元素分支。
        class _ : Sink<TSource>, IObserver<TSource>
        {
            private readonly FirstAsync<TSource> _parent;

            public _(FirstAsync<TSource> parent, IObserver<TSource> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            public void OnNext(TSource value)
            {
                base._observer.OnNext(value);
                base._observer.OnCompleted();
                base.Dispose();
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            // 观察序列结束调用OnCompleted(),根据 flag _throwOnEmpty 判断是否抛出异常，不抛出异常则返回默认值。
            public void OnCompleted()
            {
                if (_parent._throwOnEmpty)
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                }
                else
                {
                    base._observer.OnNext(default(TSource));
                    base._observer.OnCompleted();
                }

                base.Dispose();
            }
        }

        //返回观察序列中符合条件predicate的第一个元素
        class FirstAsyncImpl : Sink<TSource>, IObserver<TSource>
        {
            private readonly FirstAsync<TSource> _parent;

            public FirstAsyncImpl(FirstAsync<TSource> parent, IObserver<TSource> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            public void OnNext(TSource value)
            {
                //  gurad 标志有无符合要求的观察元素。
                var b = false;

                try
                {
                    b = _parent._predicate(value);
                }
                catch (Exception ex)
                {
                    base._observer.OnError(ex);
                    base.Dispose();
                    return;
                }

                if (b)
                {
                    base._observer.OnNext(value);
                    base._observer.OnCompleted();
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
                if (_parent._throwOnEmpty)
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_MATCHING_ELEMENTS));
                }
                else
                {
                    base._observer.OnNext(default(TSource));
                    base._observer.OnCompleted();
                }

                base.Dispose();
            }
        }
    }
}
#endif