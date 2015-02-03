// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{   
    /// <summary>
    /// Observable.IsEmpty() 的底层实现，用于判断观察序列是否为空。
    /// IsEmpty class is a great example showing the basic tricks in developing new Rx functionalities.
    /// Rx is based on the IObservable interface just like LINQ-to-Objects is based on the IEnumerable interface.
    /// http://en.wikipedia.org/wiki/Extension_method
    /// http://en.wikipedia.org/wiki/Fluent_interface
    /// By using the extended methods language feature, fluent style calling is as easy as following example
    /// var ns=source.Where(x=>x>0).Select(x=>x*2);
    /// http://en.wikipedia.org/wiki/Syntactic_sugar
    /// The SQL style LINQ is just a syntactic sugar, but makes the previous code more SQL feeling
    /// var ns=from x in source
    ///        where x>0
    ///        select x*2;
    /// note that in previous code, the souce object is of IObservable/IEnumerable interface
    /// the Where attached to an object of IObservable/IEnumerable interface
    /// and return a new object of IObservable/IEnumerable interface
    /// so are many other Rx/LINQ functions
    /// Therefore, most functions we want to develop are functions like Select and Where
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// to implement functions like Select and Where we are actually implementing a class, which is of IObservable interface
    /// note Producer is a class of IObservable interface
    class IsEmpty<TSource> : Producer<bool>
    {
        private readonly IObservable<TSource> _source;

    /// and its constructor need a parameter of the original source
        public IsEmpty(IObservable<TSource> source)
        {
    /// we store the original souce as a private field
            _source = source;
        }

    /// the Producer class defines a Run method to take care of calling chain of Subscribe
        protected override IDisposable Run(IObserver<bool> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
    /// the idea is simple
    /// we create a bridging object which can talk to the real data consumer _oberserver
            var sink = new _(observer, cancel);
            setSink(sink);
    /// we subscribe the data source with the bridging object and return the dispose handle
            return _source.SubscribeSafe(sink);
        }

    /// note that the bridging object must act as a data consumer which is of IObserver interface
    /// the so Sink class is the template to bridging
        class _ : Sink<bool>, IObserver<TSource>
        {
            public _(IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }

    /// as an data consumer the bridging object must be able to listen to data source's OnNext calling
            public void OnNext(TSource value)
            {
    /// when the data source calls, a datum is pushed in
    /// normally we do some work here
    /// after knowing what to tell the real data consumer _oberserver
    /// we call its OnNext method to transmit the processed datum
    /// in the case since we got a datum, the source is not emtpy
                base._observer.OnNext(false);
                base._observer.OnCompleted();
    /// after the transmition we dispose the bridging object
                base.Dispose();
            }

    /// OnError is similar
            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

    /// so is OnCompleted
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
