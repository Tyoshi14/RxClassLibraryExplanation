// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive
{
    abstract class TailRecursiveSink<TSource> : Sink<TSource>, IObserver<TSource>
    {
        public TailRecursiveSink(IObserver<TSource> observer, IDisposable cancel)
            : base(observer, cancel)
        {
        }

        private bool _isDisposed;
        private SerialDisposable _subscription;
        private AsyncLock _gate;
        private Stack<IEnumerator<IObservable<TSource>>> _stack;
        private Stack<int?> _length;
        protected Action _recurse;

        // A　more method is added compared to Class Sink.
        public IDisposable Run(IEnumerable<IObservable<TSource>> sources)
        {
            _isDisposed = false;
            _subscription = new SerialDisposable();
            _gate = new AsyncLock();
            _stack = new Stack<IEnumerator<IObservable<TSource>>>();
            _length = new Stack<int?>();

            var e = default(IEnumerator<IObservable<TSource>>);
            if (!TryGetEnumerator(sources, out e))
                return Disposable.Empty;

            _stack.Push(e);
            _length.Push(Helpers.GetLength(sources));

            /// <summary>
            /// Function Schedule
            ///     Schedules an action to be executed recursively.
            /// </summary>
            var cancelable = SchedulerDefaults.TailRecursion.Schedule(self =>
            {
                _recurse = self;
                _gate.Wait(MoveNext);
            });

            return new CompositeDisposable(_subscription, cancelable, Disposable.Create(() => _gate.Wait(Dispose)));
        }

        protected abstract IEnumerable<IObservable<TSource>> Extract(IObservable<TSource> source);

        private void MoveNext()
        {
            var hasNext = false;
            var next = default(IObservable<TSource>);

            do
            {
                if (_stack.Count == 0)
                    break;

                if (_isDisposed)
                    return;
                // Funtion Peek
                //Returns the object at the top of the System.Collections.Generic.Stack<T>
                //     without removing it.
                var e = _stack.Peek();
                var l = _length.Peek();

                var current = default(IObservable<TSource>);
                try
                {
                    hasNext = e.MoveNext();
                    // Gets the element in the collection at the current position of the enumerator.
                    if (hasNext)
                        current = e.Current;
                }
                catch (Exception ex)
                {
                    e.Dispose();

                    base._observer.OnError(ex);
                    base.Dispose();
                    return;
                }

                if (!hasNext)
                {
                    e.Dispose();

                    // Function:Pop  Removes and returns the object at the top of the System.Collections.Generic.Stack<T>.
                    _stack.Pop();
                    _length.Pop();
                }
                else
                {
                    // Change the number in the _length stack
                    var r = l - 1;
                    _length.Pop();
                    _length.Push(r);

                    try
                    {
                        next = Helpers.Unpack(current);
                    }
                    catch (Exception exception)
                    {
                        e.Dispose();
                        base._observer.OnError(exception);
                        base.Dispose();
                        return;
                    }

                    //
                    // Tail recursive case; drop the current frame.
                    //
                    if (r == 0)
                    {
                        e.Dispose();
                        _stack.Pop();
                        _length.Pop();
                    }

                    //
                    // Flattening of nested sequences. Prevents stack overflow in observers.
                    //
                    var nextSeq = Extract(next);
                    if (nextSeq != null)
                    {
                        var nextEnumerator = default(IEnumerator<IObservable<TSource>>);
                        if (!TryGetEnumerator(nextSeq, out nextEnumerator))
                            return;

                        _stack.Push(nextEnumerator);
                        _length.Push(Helpers.GetLength(nextSeq));

                        hasNext = false;
                    }
                }
            } while (!hasNext);

            if (!hasNext)
            {
                Done();
                return;
            }

            var d = new SingleAssignmentDisposable();
            _subscription.Disposable = d;
            d.Disposable = next.SubscribeSafe(this);
        }

        private new void Dispose()
        {
            while (_stack.Count > 0)
            {
                var e = _stack.Pop();
                _length.Pop();

                e.Dispose();
            }

            _isDisposed = true;
        }

        private bool TryGetEnumerator(IEnumerable<IObservable<TSource>> sources, out IEnumerator<IObservable<TSource>> result)
        {
            try
            {
                result = sources.GetEnumerator();
                return true;
            }
            catch (Exception exception)
            {
                base._observer.OnError(exception);
                base.Dispose();

                result = null;
                return false;
            }
        }

        public abstract void OnCompleted();
        public abstract void OnError(Exception error);
        public abstract void OnNext(TSource value);

        protected virtual void Done()
        {
            base._observer.OnCompleted();
            base.Dispose();
        }
    }
}
#endif