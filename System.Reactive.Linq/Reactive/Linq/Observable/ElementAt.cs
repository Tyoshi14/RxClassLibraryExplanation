// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    class ElementAt<TSource> : Producer<TSource>
    {
        private readonly IObservable<TSource> _source;
        private readonly int _index;
        private readonly bool _throwOnEmpty;

        public ElementAt(IObservable<TSource> source, int index, bool throwOnEmpty)
        {
            _source = source;
            _index = index;
            _throwOnEmpty = throwOnEmpty;
        }

        protected override IDisposable Run(IObserver<TSource> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<TSource>, IObserver<TSource>
        {
            private readonly ElementAt<TSource> _parent;
            private int _i;

            public _(ElementAt<TSource> parent, IObserver<TSource> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
                _i = _parent._index;
            }

            public void OnNext(TSource value)
            {
                if (_i == 0)
                {
                    // 定位到指定的位置后，返回所在位置的元素，并且结束观察。
                    base._observer.OnNext(value);
                    base._observer.OnCompleted();
                    base.Dispose();
                }

                _i--;
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                // flag _throwOnEmpty 控制Index越界是否抛出异常。若不抛出异常，返回TSource默认值default(TSource)。
                if (_parent._throwOnEmpty)
                {
                    base._observer.OnError(new ArgumentOutOfRangeException("index"));
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