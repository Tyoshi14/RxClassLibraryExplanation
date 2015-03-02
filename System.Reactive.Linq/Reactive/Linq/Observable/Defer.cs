// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Base implement class of the Observable Defer branch.
    /// Descriptions of Defer:
    ///      Returns an observable sequence that invokes the specified factory function whenever a new observer subscribes.
    /// It seems that the codes is little complex when you are trying to understand how it works.
    /// So I want to write down what I think,whether it's right or wrong, hoping that it would be useful for you.
    ///   1  When you type the code Observable.Defer(functon parameter observableFactory), it will call QueryLanguage.defer(observableFactory) first.
    ///   2  Then it calls new Defer(observableFactory) and goes into the class definition call its constructor.
    ///   3  Next it will call Defer.Subscribe(Observer) defined in class Producer.
    ///   4  Defer.Subscribe(Observer) calls Defer.Run(observer,cancel,setSink) instead.
    ///   5  Defer.Run(observer,cancel,setSink) creates a new inner class sink = new _(this, observer, cancel).
    ///   6  In next step,  Defer.Run(***) calls  sink.Run() which invokes functon parameter observableFactory to create a Observable instance.
    ///         note that in the code below you can see the logic of sink.Run().
    ///   7  At last, it will call  static implement method of Observable SubscribeSafe(this repsents variable sink itself which is also an observer) 
    ///      to subcribe the source.
    ///  Those are the procedure we get an observable sequence.    
    ///
    /// ----------------------------------------------------------------------------------------------------------------------------------
    /// write in 09/02/2015  but later find it's wrong because  in step 3 parameter observer it's null and the task will stop!
    ///  ！！！ Question ！！！
    /// Now I wonder how it works althouth I know the parameter dosen't need to be initialized util being used??????? 
    ///------------------------------------------------------------------------------------------------------------------------------------
    ///So it comes to another version
    /// 1 2 remains the same
    ///   For step 3 we find that we don't have an observable sequence. 
    ///   Unlike other static functions in Aggregate. It will generate an observable sequence first.
    ///   In order to fully understand how Defer works, I need to find an example later.
    /// </summary>
    class Defer<TValue> : Producer<TValue>, IEvaluatableObservable<TValue>
    {
        /// Func<IObservable<TValue>> means it takes no parameters but return a IObservable<TValue> instance.
        private readonly Func<IObservable<TValue>> _observableFactory;

        public Defer(Func<IObservable<TValue>> observableFactory)
        {
            _observableFactory = observableFactory;
        }

        protected override IDisposable Run(IObserver<TValue> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            //  Unlike other functions in Aggregate, we don't call  return _source.SubscribeSafe(sink) but call another inner class function 
            //  sink.Run() instead.
            return sink.Run();
        }

        public IObservable<TValue> Eval()
        {
            return _observableFactory();
        }

        class _ : Sink<TValue>, IObserver<TValue>
        {
            private readonly Defer<TValue> _parent;

            public _(Defer<TValue> parent, IObserver<TValue> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            /// <summary>
            /// The logic of Run()
            ///   Initialize an default Ibervable object result of type TValue.
            ///   redifine variable result use the specified factory function _observableFactory.
            ///   if no errors
            ///     the observer itself subcribes Observable sequence result.
            ///   else
            ///     call error function
            ///     return Disposable.Empty.
            /// </summary>
            /// <returns></returns>
            public IDisposable Run()
            {
                var result = default(IObservable<TValue>);
                try
                {
                    result = _parent.Eval();
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return Disposable.Empty;
                }

                return result.SubscribeSafe(this);
            }

            public void OnNext(TValue value)
            {
                base._observer.OnNext(value);
            }

            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }

            public void OnCompleted()
            {
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif