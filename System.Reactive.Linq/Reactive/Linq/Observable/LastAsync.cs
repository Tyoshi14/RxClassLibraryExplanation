// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Observable.LastAsync()底层实现。分为两种情况，一是直接返回观察序列的最后一个元素；二是，返回观察序列符合predicate条件的最后一个元素。 
    /// 同时也考虑了未找到指定元素，是否抛出异常或者返回默认值的处理。
    /// </summary>
    /// LastAsync 内部类实现过程中增加了标记变量 _seenValue 来标记是不是含有元素，省去了判空的过程。逻辑上更加严谨。
    /// <typeparam name="TSource"></typeparam>
    class LastAsync<TSource> : Producer<TSource>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, bool> _predicate;
        private readonly bool _throwOnEmpty;

        public LastAsync(IObservable<TSource> source, Func<TSource, bool> predicate, bool throwOnEmpty)
        {
            _source = source;
            _predicate = predicate;
            _throwOnEmpty = throwOnEmpty;
        }

        protected override IDisposable Run(IObserver<TSource> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            if (_predicate != null)
            {
                var sink = new LastAsyncImpl(this, observer, cancel);
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

        // 直接返回观察序列的最后一个元素
        class _ : Sink<TSource>, IObserver<TSource>
        {
            private readonly LastAsync<TSource> _parent;
            private TSource _value;
            // guard 用来标记观察序列是否为空。
            private bool _seenValue;

            public _(LastAsync<TSource> parent, IObserver<TSource> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;

                _value = default(TSource);
                _seenValue = false;
            }

            public void OnNext(TSource value)
            {
                _value = value;
                _seenValue = true;
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                if (!_seenValue && _parent._throwOnEmpty)
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                }
                else
                {
                    base._observer.OnNext(_value);
                    base._observer.OnCompleted();
                }

                base.Dispose();
            }
        }

        //返回观察序列符合predicate条件的最后一个元素
        class LastAsyncImpl : Sink<TSource>, IObserver<TSource>
        {
            private readonly LastAsync<TSource> _parent;
            private TSource _value;
            // guard 用于标记是不是很有 predicate 条件的元素。
            private bool _seenValue;

            public LastAsyncImpl(LastAsync<TSource> parent, IObserver<TSource> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;

                _value = default(TSource);
                _seenValue = false;
            }

            public void OnNext(TSource value)
            {
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
                    _value = value;
                    _seenValue = true;
                }
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                if (!_seenValue && _parent._throwOnEmpty)
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_MATCHING_ELEMENTS));
                }
                else
                {
                    base._observer.OnNext(_value);
                    base._observer.OnCompleted();
                }

                base.Dispose();
            }
        }
    }
}
#endif