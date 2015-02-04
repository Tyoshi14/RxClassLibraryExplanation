// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Base implement class of the SequenceEqual branch of Observable class.
    /// SequenceEqual class needs to deal with 2 conditions. One is compare two observable sequences, the other is to compare a observable sequence and a enumerable sequence.
    /// 
    /// In the following code, we can see that the structure  is a bit different from the structures before. The diffrence lays in the function Run().
    /// In other structures we subscribe the data in the override Run function. 
    /// And here observer subscribes the data in the inner bridge class called _ and  SequenceEqualImpl. The Override Run function only call the inner Run function.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    class SequenceEqual<TSource> : Producer<bool>
    {
        private readonly IObservable<TSource> _first;
        private readonly IObservable<TSource> _second;
        private readonly IEnumerable<TSource> _secondE;
        private readonly IEqualityComparer<TSource> _comparer;

        public SequenceEqual(IObservable<TSource> first, IObservable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            _first = first;
            _second = second;
            _comparer = comparer;
        }

        public SequenceEqual(IObservable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            _first = first;
            _secondE = second;
            _comparer = comparer;
        }

        protected override IDisposable Run(IObserver<bool> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            // 1 branch:  _second != null means that the second sequence is an Observable sequence.
            if (_second != null)
            {
                var sink = new _(this, observer, cancel);
                setSink(sink);
                // Pay attentiont to the change here.
                return sink.Run();
            }
            // The other is that the second sequence is an Enumerable sequence.
            else
            {
                var sink = new SequenceEqualImpl(this, observer, cancel);
                setSink(sink);
                return sink.Run();
            }
        }

        /// <summary>
        /// We are all curious about the algorithm of comparing two observable sequences. Then see the explantions below.
        /// 
        /// 1. 
        /// Initialize the paremeter.
        ///     object _gate :  realize a mutual-exclusion lock.
        ///     bool _donel ： indicate weather the first observable sequence reaches the end.
        ///     Queue<TSource> _ql ： the elements of the first  observable sequence  need to be process.
        ///     bool _doner : indicate weather the second observable sequence reaches the end.
        ///     Queue<TSource> _qr : the elements of the second  observable sequence  need to be process.
        ///     
        /// 2. 
        /// The observer of class F subscribes the first Observable sequence data. F is an observer to compare two elements from two sequences respectively.
        ///   The OnNext procedure of F
        ///     foreach value in First Observable sequence
        ///      lock
        ///         if _qr.Count >0
        ///             v=_qr.Dequeue 
        ///             equal=_comparer.Equals(value, v)
        ///             if equal is false 
        ///                 End observe, return Not Equal
        ///        else if _doner is true
        ///              End observe, return Not Equal
        ///        else 
        ///             _ql.Enqueue(value)
        ///  
        ///   The OnCompleted procedure of F 
        ///      lock
        ///         _donel=true
        ///         if _ql.Count == 0
        ///            if _qr.Count > 0
        ///                End observe, return Not Equal
        ///            else if _doner is ture
        ///                End observe, return  Equal
        ///                
        /// 3.       
        /// The observer of class S subscribes the first Observable sequence data. S is also an observer to compare two elements from two sequences respectively.
        ///   The OnNext procedure of S
        ///     foreach value in Second Observable sequence
        ///      lock
        ///         if _ql.Count >0
        ///             v=_ql.Dequeue 
        ///             equal=_comparer.Equals(value, v)
        ///             if equal is false 
        ///                 End observe, return Not Equal
        ///        else if _donel is true
        ///              End observe, return Not Equal
        ///        else 
        ///             _qr.Enqueue(value)
        ///  
        ///   The OnCompleted procedure of S 
        ///      lock
        ///         _doner=true
        ///         if _qr.Count == 0
        ///            if _ql.Count > 0
        ///                End observe, return Not Equal
        ///            else if _donel is ture
        ///                End observe, return  Equal
        ///   
        /// Conclusion:
        ///  Then we can see that the code use DataType  Queue to guarantee the order of all sequence. 
        ///  S and F work alternatively to compare the the the values to judege the equality.
        /// </summary>
        class _ : Sink<bool>
        {
            private readonly SequenceEqual<TSource> _parent;

            public _(SequenceEqual<TSource> parent, IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            private object _gate;
            private bool _donel;
            private bool _doner;
            private Queue<TSource> _ql;
            private Queue<TSource> _qr;

            public IDisposable Run()
            {
                _gate = new object();
                _donel = false;
                _doner = false;
                _ql = new Queue<TSource>();
                _qr = new Queue<TSource>();

                return new CompositeDisposable
                {
                    _parent._first.SubscribeSafe(new F(this)),
                    _parent._second.SubscribeSafe(new S(this))
                };
            }

            class F : IObserver<TSource>
            {
                private readonly _ _parent;

                public F(_ parent)
                {
                    _parent = parent;
                }

                public void OnNext(TSource value)
                {
                 /// Explain the key word lock
                 /// The lock keyword marks a statement block as a critical section by obtaining the mutual-exclusion lock for
                 /// a given objectThe lock keyword marks a statement block as a critical section by obtaining the mutual-exclusion lock for a given object
                    lock (_parent._gate)
                    {
                        if (_parent._qr.Count > 0)
                        {
                            var equal = false;
                            var v = _parent._qr.Dequeue();
                            try
                            {
                                equal = _parent._parent._comparer.Equals(value, v);
                            }
                            catch (Exception exception)
                            {
                                _parent._observer.OnError(exception);
                                _parent.Dispose();
                                return;
                            }
                            if (!equal)
                            {
                                _parent._observer.OnNext(false);
                                _parent._observer.OnCompleted();
                                _parent.Dispose();
                            }
                        }
                        else if (_parent._doner)
                        {
                            _parent._observer.OnNext(false);
                            _parent._observer.OnCompleted();
                            _parent.Dispose();
                        }
                        else
                            _parent._ql.Enqueue(value);
                    }
                }

                public void OnError(Exception error)
                {
                    _parent._observer.OnError(error);
                    _parent.Dispose();
                }

                public void OnCompleted()
                {
                    lock (_parent._gate)
                    {
                        _parent._donel = true;
                        if (_parent._ql.Count == 0)
                        {
                            if (_parent._qr.Count > 0)
                            {
                                _parent._observer.OnNext(false);
                                _parent._observer.OnCompleted();
                                _parent.Dispose();
                            }
                            else if (_parent._doner)
                            {
                                _parent._observer.OnNext(true);
                                _parent._observer.OnCompleted();
                                _parent.Dispose();
                            }
                        }
                    }
                }
            }

            class S : IObserver<TSource>
            {
                private readonly _ _parent;

                public S(_ parent)
                {
                    _parent = parent;
                }

                public void OnNext(TSource value)
                {
                    lock (_parent._gate)
                    {
                        if (_parent._ql.Count > 0)
                        {
                            var equal = false;
                            var v = _parent._ql.Dequeue();
                            try
                            {
                                equal = _parent._parent._comparer.Equals(v, value);
                            }
                            catch (Exception exception)
                            {
                                _parent._observer.OnError(exception);
                                _parent.Dispose();
                                return;
                            }
                            if (!equal)
                            {
                                _parent._observer.OnNext(false);
                                _parent._observer.OnCompleted();
                                _parent.Dispose();
                            }
                        }
                        else if (_parent._donel)
                        {
                            _parent._observer.OnNext(false);
                            _parent._observer.OnCompleted();
                            _parent.Dispose();
                        }
                        else
                            _parent._qr.Enqueue(value);
                    }
                }

                public void OnError(Exception error)
                {
                    _parent._observer.OnError(error);
                    _parent.Dispose();
                }

                public void OnCompleted()
                {
                    lock (_parent._gate)
                    {
                        _parent._doner = true;
                        if (_parent._qr.Count == 0)
                        {
                            if (_parent._ql.Count > 0)
                            {
                                _parent._observer.OnNext(false);
                                _parent._observer.OnCompleted();
                                _parent.Dispose();
                            }
                            else if (_parent._donel)
                            {
                                _parent._observer.OnNext(true);
                                _parent._observer.OnCompleted();
                                _parent.Dispose();
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Compare an Observable sequence with an enumerable sequence.
        /// The basic thought of this class is that call OnNext and _enumerator.Current together. 
        /// Then return the result of the compare the result. 
        /// </summary>
        class SequenceEqualImpl : Sink<bool>, IObserver<TSource>
        {
            private readonly SequenceEqual<TSource> _parent;

            public SequenceEqualImpl(SequenceEqual<TSource> parent, IObserver<bool> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            private IEnumerator<TSource> _enumerator;

            public IDisposable Run()
            {
                //
                // Notice the evaluation order of obtaining the enumerator and subscribing to the
                // observable sequence is reversed compared to the operator's signature. This is
                // required to make sure the enumerator is available as soon as the observer can
                // be called. Otherwise, we end up having a race for the initialization and use
                // of the _rightEnumerator field.
                //
                try
                {
                    _enumerator = _parent._secondE.GetEnumerator();
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return Disposable.Empty;
                }

                return new CompositeDisposable(
                    _parent._first.SubscribeSafe(this),
                    _enumerator
                );
            }

            public void OnNext(TSource value)
            {
                var equal = false;

                try
                {
                    if (_enumerator.MoveNext())
                    {
                        var current = _enumerator.Current;
                        equal = _parent._comparer.Equals(value, current);
                    }
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return;
                }

                if (!equal)
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
                var hasNext = false;

                try
                {
                    hasNext = _enumerator.MoveNext();
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return;
                }

                base._observer.OnNext(!hasNext);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif