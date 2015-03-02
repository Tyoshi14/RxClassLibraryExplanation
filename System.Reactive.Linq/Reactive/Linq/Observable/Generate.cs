// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Base implement class of the Observable Generate branch
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    class Generate<TState, TResult> : Producer<TResult>
    {
        private readonly TState _initialState;
        private readonly Func<TState, bool> _condition;
        private readonly Func<TState, TState> _iterate;
        private readonly Func<TState, TResult> _resultSelector;
//     Represents a point in time, typically expressed as a date and time of day,
//     relative to Coordinated Universal Time (UTC).
        private readonly Func<TState, DateTimeOffset> _timeSelectorA;
// Summary:
//     Represents a time interval.
        private readonly Func<TState, TimeSpan> _timeSelectorR;
        private readonly IScheduler _scheduler;

        // 3 constructors
        public Generate(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, IScheduler scheduler)
        {
            _initialState = initialState;
            _condition = condition;
            _iterate = iterate;
            _resultSelector = resultSelector;
            _scheduler = scheduler;
        }

        public Generate(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, DateTimeOffset> timeSelector, IScheduler scheduler)
        {
            _initialState = initialState;
            _condition = condition;
            _iterate = iterate;
            _resultSelector = resultSelector;
            _timeSelectorA = timeSelector;
            _scheduler = scheduler;
        }

        public Generate(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, TimeSpan> timeSelector, IScheduler scheduler)
        {
            _initialState = initialState;
            _condition = condition;
            _iterate = iterate;
            _resultSelector = resultSelector;
            _timeSelectorR = timeSelector;
            _scheduler = scheduler;
        }

        // The Run() function must to override in Producer. When a certain observer subscribes some observable object, Run will be called.
        // It is divided into 3 cases acoording to 3 different constructors.
        protected override IDisposable Run(IObserver<TResult> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            if (_timeSelectorA != null)
            {
                var sink = new SelectorA(this, observer, cancel);
                setSink(sink);
                return sink.Run();
            }
            else if (_timeSelectorR != null)
            {
                var sink = new Delta(this, observer, cancel);
                setSink(sink);
                return sink.Run();
            }
            else
            {
                var sink = new _(this, observer, cancel);
                setSink(sink);
                return sink.Run();
            }
        }

        class SelectorA : Sink<TResult>
        {
            private readonly Generate<TState, TResult> _parent;

            public SelectorA(Generate<TState, TResult> parent, IObserver<TResult> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            private bool _first;
            private bool _hasResult;
            private TResult _result;

            public IDisposable Run()
            {
                _first = true;
                _hasResult = false;
                _result = default(TResult);

                return _parent._scheduler.Schedule(_parent._initialState, InvokeRec);
            }

            private IDisposable InvokeRec(IScheduler self, TState state)
            {
                var time = default(DateTimeOffset);

                if (_hasResult)
                    base._observer.OnNext(_result);
                try
                {
                    if (_first)
                        _first = false;
                    else
                        state = _parent._iterate(state);
                    _hasResult = _parent._condition(state);
                    if (_hasResult)
                    {
                        _result = _parent._resultSelector(state);
                        time = _parent._timeSelectorA(state);
                    }
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return Disposable.Empty;
                }

                if (!_hasResult)
                {
                    base._observer.OnCompleted();
                    base.Dispose();
                    return Disposable.Empty;
                }
                // Just like a loop.
                return self.Schedule(state, time, InvokeRec);
            }
        }

        class Delta : Sink<TResult>
        {
            private readonly Generate<TState, TResult> _parent;

            public Delta(Generate<TState, TResult> parent, IObserver<TResult> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            private bool _first;
            private bool _hasResult;
            private TResult _result;

            public IDisposable Run()
            {
                _first = true;
                _hasResult = false;
                _result = default(TResult);

                return _parent._scheduler.Schedule(_parent._initialState, InvokeRec);
            }

            private IDisposable InvokeRec(IScheduler self, TState state)
            {
                var time = default(TimeSpan);

                if (_hasResult)
                    base._observer.OnNext(_result);
                try
                {
                    if (_first)
                        _first = false;
                    else
                        state = _parent._iterate(state);
                    _hasResult = _parent._condition(state);
                    if (_hasResult)
                    {
                        _result = _parent._resultSelector(state);
                        time = _parent._timeSelectorR(state);
                    }
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return Disposable.Empty;
                }

                if (!_hasResult)
                {
                    base._observer.OnCompleted();
                    base.Dispose();
                    return Disposable.Empty;
                }

                // Realize a loop.
                return self.Schedule(state, time, InvokeRec);
            }
        }

        class _ : Sink<TResult>
        {
            private readonly Generate<TState, TResult> _parent;

            public _(Generate<TState, TResult> parent, IObserver<TResult> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            private TState _state;
            private bool _first;

            public IDisposable Run()
            {
                _state = _parent._initialState;
                _first = true;

                // Returns the ISchedulerLongRunning implementation of the specified scheduler, or null if no such implementation is available.
                var longRunning = _parent._scheduler.AsLongRunning();
                if (longRunning != null)
                {
                // Schedules an action to be executed.
                //  Execute function Loop, but I can't see it takes any parameter? Why???????
                //      private void Loop(ICancelable cancel) takes ICancelable parameter, and ICancelable is derived from IDisposable.
                //      You can also find that we have another variable IDisposable cancel.
                //      So I think the code takes IDisposable cancel as input automatically. Is it right???
                //                                        --------- Question Remained!
                    return longRunning.ScheduleLongRunning(Loop);
                }
                else
                {
                    return _parent._scheduler.Schedule(LoopRec);
                }
            }

            private void Loop(ICancelable cancel)
            {
                /// IsDisposed gets a value that indicates whether the object is disposed.
                /// As for the logic of these code, we can s ee that
                /// 
                /// while source can't dispose
                ///     initialize the result flag(first false) that indicates whether the condition function generate a result or not.
                ///     If _first eq true
                ///       _first eq false
                ///     else
                ///       generate another element _state to be checked
                ///       Judge _state by using the _condition function, then assign the result to hasResult
                ///       If hasResult eq true
                ///         select the result
                ///         Push the result to Observer
                /// 
                /// When error occurs in procedure above, that is to say source still can't dispose.
                ///      Dispose the source.
                ///        
                ///     
                while (!cancel.IsDisposed)
                {
                    var hasResult = false;
                    var result = default(TResult);
                    try
                    {
                        if (_first)
                            _first = false;
                        else
                            _state = _parent._iterate(_state);
                        hasResult = _parent._condition(_state);
                        if (hasResult)
                            result = _parent._resultSelector(_state);
                    }
                    catch (Exception exception)
                    {
                        base._observer.OnError(exception);
                        base.Dispose();
                        return;
                    }

                    if (hasResult)
                        base._observer.OnNext(result);
                    else
                        break;
                }

                if (!cancel.IsDisposed)
                    base._observer.OnCompleted();

                base.Dispose();
            }

            private void LoopRec(Action recurse)
            {
            /// The logic is almost the same witn Loop
            /// The difference is that it involves a recurse function!
                var hasResult = false;
                var result = default(TResult);
                try
                {
                    if (_first)
                        _first = false;
                    else
                        _state = _parent._iterate(_state);
                    hasResult = _parent._condition(_state);
                    if (hasResult)
                        result = _parent._resultSelector(_state);
                }
                catch (Exception exception)
                {
                    base._observer.OnError(exception);
                    base.Dispose();
                    return;
                }

                if (hasResult)
                {
                    base._observer.OnNext(result);
                /// What does this function do? Where is it from? What is the details of Action recurse? 
                /// Question remianed!!!!
                    recurse();
                }
                else
                {
                    base._observer.OnCompleted();
                    base.Dispose();
                }
            }
        }
    }
}
#endif