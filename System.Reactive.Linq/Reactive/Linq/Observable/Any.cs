// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    class Any<TSource> : Producer<bool>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, bool> _predicate;

        /// Any存在两个构造函数。
        /// 区别在于有无参数 Func<TSource, bool> predicate。
         
        public Any(IObservable<TSource> source)
        {
            _source = source;
        }

        public Any(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        /// <summary>
        /// Observable.Any()功能的底层实现。
        /// 如Observable 方法声明中的描述一样，Any方法存在两种情况，一：判断观察序列是否为空。二： 观察序列元素中有无符合 predicate 情况的元素。
        /// </summary>
        protected override IDisposable Run(IObserver<bool> observer, IDisposable cancel, Action<IDisposable> setSink)
        {

            if (_predicate != null)
            {
                var sink = new AnyImpl(this, observer, cancel);
                setSink(sink);
                return _source.SubscribeSafe(sink);
            }
            else
            {
                var sink = new _(observer, cancel);
                setSink(sink);
                return _source.SubscribeSafe(sink);
            }
        }

        /// <summary>
        /// 条件 predicate 为null的处理分支。
        /// </summary>
        class _ : Sink<bool>, IObserver<TSource>
        {
            public _(IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }
            // 发现存在元素，即不为空，直接Observer赋值 true,结束观察。
            public void OnNext(TSource value)
            {
                base._observer.OnNext(true);
                base._observer.OnCompleted();
                base.Dispose();
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            // 未调用 OnNext, 即观察序列空，Observer 赋值 false, 结束观察。
            public void OnCompleted()
            {
                base._observer.OnNext(false);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }

        /// <summary>
        ///  条件 predicate 不为null，即含有判断条件的处理分支。
        /// </summary>
        class AnyImpl : Sink<bool>, IObserver<TSource>
        {
            private readonly Any<TSource> _parent;

            public AnyImpl(Any<TSource> parent, IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            public void OnNext(TSource value)
            {
                // The sentry
                var res = false;
                try
                {
                    res = _parent._predicate(value);
                }
                catch (Exception ex)
                {
                    base._observer.OnError(ex);
                    base.Dispose();
                    return;
                }

                // 根据 sentry 的监视结果， 发现符合 predicate 条件的元素立刻赋值Obsever为true，结束观察。
                if (res)
                {
                    base._observer.OnNext(true);
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
                // 观察序列结束后，为发现符合条件的元素，赋值Obsever为false，结束观察。
                base._observer.OnNext(false);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif