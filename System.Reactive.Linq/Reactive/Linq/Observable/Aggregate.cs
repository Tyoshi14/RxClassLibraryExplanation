// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
    // Aggregate 继承Producer类 ，是Observable.Aggregate() 方法的底层实现。 
    // Producer 类定义在两个不同的地方，但是实现方法和原理是类似的。
    //Producer类的描述为：Base class for implementation of query operators, providing performance benefits over the use of Observable.Create。
    // Producer类只包含两个方法 IDisposable Subscribe() 和 abstract IDisposable Run() .
    /// <Conclusion>
    /// 总结Aggregate部分的实现原理。
    /// 首先，Aggregates实现是IObservable接口对象，故其能够实现对IObservable方法的扩展。
    ///      继承Producer（实现IObservable接口），实现了观察序列的基本功能 Subscribe.
    /// 其次，重载 Producer对象的扩展功能 Run，该函数是 Aggregate 的核心函数。其实现思路为：
    ///      定义了一个内部类 _ ,该类继承Sink 类。
    ///          Sink 类 为一个 包含 IObserver接口对象的 IDisposable 对象，实现了释放 Observer 对象的作用。
    ///      内部类 _ 实现 IObserver 接口，同时包含了 Aggregates 对象。故可调用 Aggregates 对象中 IObserver 类型的成员变量。
    ///         IObserver 接口 通过调用 Aggregates 对象IObserver 成员变量的相应接口实现方法方法实现。
    ///      函数式编程允许将函数作为参数，在另一个函数中调用。 Action<T> 可以表达为 Option<T>->(), 可将对象转换为一个delegate.
    /// 最后，通过  IObservable 的 Subscrible（IObserver） 方法 实现 资源的订阅。
    /// Aggregates 不仅仅是一个 IObservable类型的对象， 同时它集合了 IObserver 接口的功能，在实现订阅资源的基础上，融入了特定的方法。
    ///                                                                                                         ---By Tyoshi
    /// </Couclusion>
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

     
        ///  每个Producer实现类都必须重载的函数， 首先解释三个参数的含义.
        ///  observer :  观察者
        ///  IDisposable cancel ：Run（）函数的返回对象，用来释放资源。 
        ///  Action<IDisposable> setSink : 提供回调方法，使得订阅者获得缓冲池内的对象，进而可以实现结束进程的作用。
        protected override IDisposable Run(IObserver<TResult> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
        // 封装 _ 对象, 使对象变成一个delegate的代理方法。
            setSink(sink);
        // 调用 ObservableExtensions 类中的SubscribeSafe() 方法, 订阅一个资源。
       //ObservableExtensions provides a set of static methods for subscribing delegates to observables.
       // subscribesafe的描述：  Subscribes to the specified source, re-routing synchronous exceptions during invocation of the Subscribe method to the observer's OnError channel.
            return _source.SubscribeSafe(sink);
        }

        // _ 为 Aggregate<TSource, TAccumulate, TResult> 的内部类,继承Sink类和IObserver接口。同时实现 IObserver和　mute the outgoing observer的功能。 
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
                //virtual是已经实现可以override的
                //abstract是未实现的必须override的
                //建议仔细阅读
                //https://msdn.microsoft.com/en-us/library/ms173152.aspx
                //https://msdn.microsoft.com/en-us/library/6fawty39.aspx
                //https://msdn.microsoft.com/en-us/library/ms173153.aspx
                base.Dispose();
            }
        }
    }


    // 两个不同的Aggregate 类，两个类之间的区别是什么？
    // 区别一：在于泛型函数的参数个数不同。
    // 区别二在于没有选择函数resultSelector。
    // 区别三，  Aggregate<TSource> 表示没有起始值 seed 传入的情况下的处理过程。
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
            // 成员变量， 标志有无起始值的传入。
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
