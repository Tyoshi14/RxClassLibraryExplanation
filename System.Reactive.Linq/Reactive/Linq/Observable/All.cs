// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    //Observable.All 底层实现。
    class All<TSource> : Producer<bool>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, bool> _predicate;

        public All(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

       /// <summary>
       // 重载 Producer 核心函数 Run（），是整个 All 部分核心。
       /// </summary>
        /// <param name="observer">观察者。这时发现一个问题，IObserver<bool> observer 对象从何而来，在代码中指向哪个对象？？ 见下面分析</param>
       /// <param name="cancel"> Run 函数返回的资源释放 接口IDisposable 实现对象</param>
       /// <param name="setSink"> Delegate 一个输入参数，没有返回结果。</param>
       /// <returns></returns>
        protected override IDisposable Run(IObserver<bool> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            // _source.SubscribeSafe(sink) 最后会调用 _source.Subscribe(Observer) 方法， 在Subscribe（）方法的实现中调用Run（observer， ， ）方法。
            // 这样，就找到了IObserver<bool> observer 对象的来源。在本段代码里面就是 sink，class _ 的一个实例。 
            // 所以 看到IObserver<bool>  与 Sink<bool> 的类型 都是 bool类型。 
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<bool>, IObserver<TSource>
        {
            private readonly All<TSource> _parent;

            public _(All<TSource> parent, IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            // 判断观察序列是不是全部符合 predicate 函数的核心实现代码。
            // 实现的基本思想是逐个观察观察序列，遇到不符合 predicate 的情况就结束程序的运行，并且返回运行结果。
            public void OnNext(TSource value)
            {
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

                // 在观察序列未空前出现不符合 predicate 情况，IObserver 赋值false，结束观察。
                if (!res)
                {
                    base._observer.OnNext(false);
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
                // 所有的观察序列元素符合 predicate， IObserver 赋值true，结束观察。
                // 通过这段代码，加强对 IObserver的了解，IObserver实现遍历 IObservable  和保存 最后返回结果的作用。
                base._observer.OnNext(true);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif