// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    // Aggregate 继承Producer类 ，是Observable.Aggregate() 方法的底层实现。 
    // Producer 类定义在两个不同的地方，但是实现方法和原理是类似的。
    //Producer类的描述为：Base class for implementation of query operators, providing performance benefits over the use of Observable.Create。
    // Producer类只包含两个方法 IDisposable Subscribe() 和 abstract IDisposable Run() .
    class Aggregate<TSource, TAccumulate, TResult> : Producer<TResult>
    {
        private readonly IObservable<TSource> _source;
        private readonly TAccumulate _seed;
        private readonly Func<TAccumulate, TSource, TAccumulate> _accumulator;
        private readonly Func<TAccumulate, TResult> _resultSelector;

        // Aggregate类的构造函数。
        public Aggregate(IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
        {
            _source = source;
            _seed = seed;
            _accumulator = accumulator;
            _resultSelector = resultSelector;
        }

     
        ///  每个Producer实现类都必须重载的函数
        protected override IDisposable Run(IObserver<TResult> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
        // 封装 _ 对象为一个方法。
            setSink(sink);
        // 调用 ObservableExtensions 类中的SubscribeSafe() 方法, 订阅一个资源。
       //ObservableExtensions provides a set of static methods for subscribing delegates to observables.
       // subscribesafe的描述：  Subscribes to the specified source, re-routing synchronous exceptions during invocation of the Subscribe method to the observer's OnError channel.
            return _source.SubscribeSafe(sink);
        }

        // _ 为 Aggregate<TSource, TAccumulate, TResult> 的内部类,继承Sink类和IObserver接口。完成？？？功能。
        // Sink类功能： Base class for implementation of query operators, providing a lightweight sink that can be disposed to mute the outgoing observer.
        class _ : Sink<TResult>, IObserver<TSource>
        {
            private readonly Aggregate<TSource, TAccumulate, TResult> _parent;
            private TAccumulate _accumulation;

            public _(Aggregate<TSource, TAccumulate, TResult> parent, IObserver<TResult> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
                _accumulation = _parent._seed;
            }

           // 实现aggregate功能的核心实现。
            public void OnNext(TSource value)
            {
                try
                {
                    _accumulation = _parent._accumulator(_accumulation, value);
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
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
                // default() 是什么意思？？
                var result = default(TResult);
                try
                {
               // 由之前 observable.arragates（） 传进来的参数 Stubs<TAccumulate>.I知，仅需要一个参数。
                    result = _parent._resultSelector(_accumulation);
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return;
                }

                // 调用基类sink的 onNext() OnCompleted() 方法。
                base._observer.OnNext(result);
                base._observer.OnCompleted();
                //  base.Dispose() s是一个 virtual 函数类型，为什么可以调用呢？？
                // virtual和abstract是不同的，C#的精妙之处。
                base.Dispose();
            }
        }
    }


    // 两个不同的Aggregate 类，两个类之间的区别是什么？
    // 区别一：在于泛型函数的参数个数不同。 区别二在于没有选择函数resultSelector
    class Aggregate<TSource> : Producer<TSource>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, TSource, TSource> _accumulator;

        public Aggregate(IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
        {
            _source = source;
            _accumulator = accumulator;
        }

        protected override IDisposable Run(IObserver<TSource> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<TSource>, IObserver<TSource>
        {
            private readonly Aggregate<TSource> _parent;
            private TSource _accumulation;
            private bool _hasAccumulation;

            public _(Aggregate<TSource> parent, IObserver<TSource> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
       //default is the keyword used to get the default value of Type. TSource is the generic type parameter. It is just the place holder for type token.
                _accumulation = default(TSource);
                _hasAccumulation = false;
            }

            public void OnNext(TSource value)
            {
                if (!_hasAccumulation)
                {
                    _accumulation = value;
                    _hasAccumulation = true;
                }
                else
                {
                    try
                    {
                        _accumulation = _parent._accumulator(_accumulation, value);
                    }
                    catch (Exception exception)
                    {
                        base._observer.OnError(exception);
                        base.Dispose();
                    }
                }
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                if (!_hasAccumulation)
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                    base.Dispose();
                }
                else
                {
                    base._observer.OnNext(_accumulation);
                    base._observer.OnCompleted();
                    base.Dispose();
                }
            }
        }
    }
}
#endif