// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl
{
    class Contains<TSource> : Producer<bool>
    {
        private readonly IObservable<TSource> _source;
        private readonly TSource _value;
        // IEqualityComparer 接口,接口描述为 Defines methods to support the comparison of objects for equality.
        private readonly IEqualityComparer<TSource> _comparer;

        public Contains(IObservable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            _source = source;
            _value = value;
            _comparer = comparer;
        }

        protected override IDisposable Run(IObserver<bool> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<bool>, IObserver<TSource>
        {
            private readonly Contains<TSource> _parent;

            public _(Contains<TSource> parent, IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            public void OnNext(TSource value)
            {
                // guard
                var res = false;
                try
                {
                // 判断观察序列中是否包含指定的值。
                    res = _parent._comparer.Equals(value, _parent._value);
                }
                catch (Exception ex)
                {
                    base._observer.OnError(ex);
                    base.Dispose();
                    return;
                }
                // guard==true.  赋值Observer为True. 结束观察。
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
                base._observer.OnNext(false);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif