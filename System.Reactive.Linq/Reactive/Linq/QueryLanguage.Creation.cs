// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Linq;

#if !NO_TPL
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
#endif

namespace System.Reactive.Linq
{
#if !NO_PERF
    using ObservableImpl;
#endif

    internal partial class QueryLanguage
    {
        #region - Create -

        public virtual IObservable<TSource> Create<TSource>(Func<IObserver<TSource>, IDisposable> subscribe)
        {
            return new AnonymousObservable<TSource>(subscribe);
        }

        /// <summary>
        /// This function is correspond to the function IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Action> subscribe).
        /// In addition, I want to say that this function shows us the use of Lambda in C#. 
        /// Lambda is powerful and very often used in function programming.
        /// </summary>
        /// <typeparam name="TSource"> The return element type</typeparam>
        /// <param name="subscribe">An delegate function which takes a IObserver object as input and a Action delegate function as output. </param>
        /// <returns></returns>
        public virtual IObservable<TSource> Create<TSource>(Func<IObserver<TSource>, Action> subscribe)
        {
            // The parameter  type of the constructor is Func<IObserver<T>, IDisposable>. so we should do some changes to the input parameter subscribe.
            return new AnonymousObservable<TSource>(o =>
            {
                // As we can see Func subscribe return an Action result which should be turn into type IDisposable.
                var a = subscribe(o);
                /// The logic of the next line is as follows 
                /// if a is null
                ///     return an empty instance created by Disposable.Empty. 
                /// else 
                ///     return a disposable object，created by Disposable.Create(a)，that invokes the specified action when disposed. 
                return a != null ? Disposable.Create(a) : Disposable.Empty;
            });
        }

        #endregion

        #region - CreateAsync -

#if !NO_TPL
      /// <summary>
      /// The function of the code below is:
      ///     Creates an observable sequence from a specified cancellable asynchronous[异步的] Subscribe method.
      /// </summary>
      /// <typeparam name="TResult"> The type of the element in the return sequence.</typeparam>
      /// <param name="subscribeAsync"> A functions delegate takes 2 parameters and 1 output result.
      ///   The first parameter is an object that implements interface IObserver.
      ///   The second parameter is a structure named CancellationToken which propagates notification that operations should be canceled.
      ///   The result is an instance of Task that represents an asynchronous operation.
      /// </param>
      /// 
        public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task> subscribeAsync)
        {
            //call constructor AnonymousObservable(Func<IObserver<T>, IDisposable> subscribe)
            // The parameter passed into the constructor is a lambda expression.
            // observer is input and the codes in {} is a procedure that can produce an IDisposable output result. 
            return new AnonymousObservable<TResult>(observer =>
            {
                // Initializes the System.Threading.CancellationTokenSource. 
                var cancellable = new CancellationDisposable();

                //  there we see a new function ToObservable() which returns an observable sequence that signals when the task completes.
                // The second parameter means getting the <see cref="T:System.Threading.CancellationToken"/> used by this CancellationDisposable.
                var taskObservable = subscribeAsync(observer, cancellable.Token).ToObservable();

                //  constructor AnonymousObserver : Creates an observer from the specified OnNext, OnError, and OnCompleted actions.
                //  new structure Unit
                //      Determines whether the specified Unit values is equal to the current Unit.
                var taskCompletionObserver = new AnonymousObserver<Unit>(Stubs<Unit>.Ignore, observer.OnError, observer.OnCompleted);

                //  Subscribe:  Notifies the provider taskObservable that an observer taskCompletionObserver is to receive notifications.
                var subscription = taskObservable.Subscribe(taskCompletionObserver);

                //   Represents a group of disposable resources that are disposed together.
                return new CompositeDisposable(cancellable, subscription);
            });
        }

        public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task> subscribeAsync)
        {
            return Create<TResult>((observer, token) => subscribeAsync(observer));
        }

        public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task<IDisposable>> subscribeAsync)
        {
            return new AnonymousObservable<TResult>(observer =>
            {
                var subscription = new SingleAssignmentDisposable();
                var cancellable = new CancellationDisposable();

                var taskObservable = subscribeAsync(observer, cancellable.Token).ToObservable();
                var taskCompletionObserver = new AnonymousObserver<IDisposable>(d => subscription.Disposable = d ?? Disposable.Empty, observer.OnError, Stubs.Nop);

                //
                // We don't cancel the subscription below *ever* and want to make sure the returned resource gets disposed eventually.
                // Notice because we're using the AnonymousObservable<T> type, we get auto-detach behavior for free.
                //
                taskObservable.Subscribe(taskCompletionObserver);

                return new CompositeDisposable(cancellable, subscription);
            });
        }

        public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task<IDisposable>> subscribeAsync)
        {
            return Create<TResult>((observer, token) => subscribeAsync(observer));
        }

        public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task<Action>> subscribeAsync)
        {
            return new AnonymousObservable<TResult>(observer =>
            {
                var subscription = new SingleAssignmentDisposable();
                var cancellable = new CancellationDisposable();

                var taskObservable = subscribeAsync(observer, cancellable.Token).ToObservable();
                var taskCompletionObserver = new AnonymousObserver<Action>(a => subscription.Disposable = a != null ? Disposable.Create(a) : Disposable.Empty, observer.OnError, Stubs.Nop);

                //
                // We don't cancel the subscription below *ever* and want to make sure the returned resource eventually gets disposed.
                // Notice because we're using the AnonymousObservable<T> type, we get auto-detach behavior for free.
                //
                taskObservable.Subscribe(taskCompletionObserver);

                return new CompositeDisposable(cancellable, subscription);
            });
        }

        public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task<Action>> subscribeAsync)
        {
            return Create<TResult>((observer, token) => subscribeAsync(observer));
        }
#endif

        #endregion

        #region + Defer +

        public virtual IObservable<TValue> Defer<TValue>(Func<IObservable<TValue>> observableFactory)
        {
#if !NO_PERF
            return new Defer<TValue>(observableFactory);
#else
            return new AnonymousObservable<TValue>(observer =>
            {
                IObservable<TValue> result;
                try
                {
                    result = observableFactory();
                }
                catch (Exception exception)
                {
                    return Throw<TValue>(exception).Subscribe(observer);
                }

                return result.Subscribe(observer);
            });
#endif
        }

        #endregion

        #region + DeferAsync +

#if !NO_TPL
        public virtual IObservable<TValue> Defer<TValue>(Func<Task<IObservable<TValue>>> observableFactoryAsync)
        {
            return Defer(() => StartAsync(observableFactoryAsync).Merge());
        }

        public virtual IObservable<TValue> Defer<TValue>(Func<CancellationToken, Task<IObservable<TValue>>> observableFactoryAsync)
        {
            return Defer(() => StartAsync(observableFactoryAsync).Merge());
        }
#endif

        #endregion

        #region + Empty +

        public virtual IObservable<TResult> Empty<TResult>()
        {
#if !NO_PERF
            return new Empty<TResult>(SchedulerDefaults.ConstantTimeOperations);
#else
            return Empty_<TResult>(SchedulerDefaults.ConstantTimeOperations);
#endif
        }

        public virtual IObservable<TResult> Empty<TResult>(IScheduler scheduler)
        {
#if !NO_PERF
            return new Empty<TResult>(scheduler);
#else
            return Empty_<TResult>(scheduler);
#endif
        }

#if NO_PERF
        private static IObservable<TResult> Empty_<TResult>(IScheduler scheduler)
        {
            return new AnonymousObservable<TResult>(observer => scheduler.Schedule(observer.OnCompleted));
        }
#endif

        #endregion

        #region + Generate +

        public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector)
        {
#if !NO_PERF
            return new Generate<TState, TResult>(initialState, condition, iterate, resultSelector, SchedulerDefaults.Iteration);
#else
            return Generate_<TState, TResult>(initialState, condition, iterate, resultSelector, SchedulerDefaults.Iteration);
#endif
        }

        public virtual IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, IScheduler scheduler)
        {
#if !NO_PERF
            return new Generate<TState, TResult>(initialState, condition, iterate, resultSelector, scheduler);
#else
            return Generate_<TState, TResult>(initialState, condition, iterate, resultSelector, scheduler);
#endif
        }

#if NO_PERF
        private static IObservable<TResult> Generate_<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, IScheduler scheduler)
        {
            return new AnonymousObservable<TResult>(observer =>
            {
                var state = initialState;
                var first = true;
                return scheduler.Schedule(self =>
                {
                    var hasResult = false;
                    var result = default(TResult);
                    try
                    {
                        if (first)
                            first = false;
                        else
                            state = iterate(state);
                        hasResult = condition(state);
                        if (hasResult)
                            result = resultSelector(state);
                    }
                    catch (Exception exception)
                    {
                        observer.OnError(exception);
                        return;
                    }

                    if (hasResult)
                    {
                        observer.OnNext(result);
                        self();
                    }
                    else
                        observer.OnCompleted();
                });
            });
        }
#endif

        #endregion

        #region + Never +

        public virtual IObservable<TResult> Never<TResult>()
        {
#if !NO_PERF
            return new Never<TResult>();
#else
            return new AnonymousObservable<TResult>(observer => Disposable.Empty);
#endif
        }

        #endregion

        #region + Range +

        public virtual IObservable<int> Range(int start, int count)
        {
            return Range_(start, count, SchedulerDefaults.Iteration);
        }

        public virtual IObservable<int> Range(int start, int count, IScheduler scheduler)
        {
            return Range_(start, count, scheduler);
        }

        private static IObservable<int> Range_(int start, int count, IScheduler scheduler)
        {
#if !NO_PERF
            return new Range(start, count, scheduler);
#else
            return new AnonymousObservable<int>(observer =>
            {
                return scheduler.Schedule(0, (i, self) =>
                {
                    if (i < count)
                    {
                        observer.OnNext(start + i);
                        self(i + 1);
                    }
                    else
                        observer.OnCompleted();
                });
            });
#endif
        }

        #endregion

        #region + Repeat +

        public virtual IObservable<TResult> Repeat<TResult>(TResult value)
        {
#if !NO_PERF
            return new Repeat<TResult>(value, null, SchedulerDefaults.Iteration);
#else
            return Repeat_(value, SchedulerDefaults.Iteration);
#endif
        }

        public virtual IObservable<TResult> Repeat<TResult>(TResult value, IScheduler scheduler)
        {
#if !NO_PERF
            return new Repeat<TResult>(value, null, scheduler);
#else
            return Repeat_<TResult>(value, scheduler);
#endif
        }

#if NO_PERF
        private IObservable<TResult> Repeat_<TResult>(TResult value, IScheduler scheduler)
        {
            return Return(value, scheduler).Repeat();
        }
#endif

        public virtual IObservable<TResult> Repeat<TResult>(TResult value, int repeatCount)
        {
#if !NO_PERF
            return new Repeat<TResult>(value, repeatCount, SchedulerDefaults.Iteration);
#else
            return Repeat_(value, repeatCount, SchedulerDefaults.Iteration);
#endif
        }

        public virtual IObservable<TResult> Repeat<TResult>(TResult value, int repeatCount, IScheduler scheduler)
        {
#if !NO_PERF
            return new Repeat<TResult>(value, repeatCount, scheduler);
#else
            return Repeat_(value, repeatCount, scheduler);
#endif
        }

#if NO_PERF
        private IObservable<TResult> Repeat_<TResult>(TResult value, int repeatCount, IScheduler scheduler)
        {
            return Return(value, scheduler).Repeat(repeatCount);
        }
#endif

        #endregion

        #region + Return +

        public virtual IObservable<TResult> Return<TResult>(TResult value)
        {
#if !NO_PERF
            return new Return<TResult>(value, SchedulerDefaults.ConstantTimeOperations);
#else
            return Return_<TResult>(value, SchedulerDefaults.ConstantTimeOperations);
#endif
        }

        public virtual IObservable<TResult> Return<TResult>(TResult value, IScheduler scheduler)
        {
#if !NO_PERF
            return new Return<TResult>(value, scheduler);
#else
            return Return_<TResult>(value, scheduler);
#endif
        }

#if NO_PERF
        private static IObservable<TResult> Return_<TResult>(TResult value, IScheduler scheduler)
        {
            return new AnonymousObservable<TResult>(observer => 
                scheduler.Schedule(() =>
                {
                    observer.OnNext(value);
                    observer.OnCompleted();
                })
            );
        }
#endif

        #endregion

        #region + Throw +

        public virtual IObservable<TResult> Throw<TResult>(Exception exception)
        {
#if !NO_PERF
            return new Throw<TResult>(exception, SchedulerDefaults.ConstantTimeOperations);
#else
            return Throw_<TResult>(exception, SchedulerDefaults.ConstantTimeOperations);
#endif
        }

        public virtual IObservable<TResult> Throw<TResult>(Exception exception, IScheduler scheduler)
        {
#if !NO_PERF
            return new Throw<TResult>(exception, scheduler);
#else
            return Throw_<TResult>(exception, scheduler);
#endif
        }

#if NO_PERF
        private static IObservable<TResult> Throw_<TResult>(Exception exception, IScheduler scheduler)
        {
            return new AnonymousObservable<TResult>(observer => scheduler.Schedule(() => observer.OnError(exception)));
        }
#endif

        #endregion

        #region + Using +

        public virtual IObservable<TSource> Using<TSource, TResource>(Func<TResource> resourceFactory, Func<TResource, IObservable<TSource>> observableFactory) where TResource : IDisposable
        {
#if !NO_PERF
            return new Using<TSource, TResource>(resourceFactory, observableFactory);
#else
            return new AnonymousObservable<TSource>(observer =>
            {
                var source = default(IObservable<TSource>);
                var disposable = Disposable.Empty;
                try
                {
                    var resource = resourceFactory();
                    if (resource != null)
                        disposable = resource;
                    source = observableFactory(resource);
                }
                catch (Exception exception)
                {
                    return new CompositeDisposable(Throw<TSource>(exception).Subscribe(observer), disposable);
                }

                return new CompositeDisposable(source.Subscribe(observer), disposable);
            });
#endif
        }

        #endregion

        #region - UsingAsync -

#if !NO_TPL

        public virtual IObservable<TSource> Using<TSource, TResource>(Func<CancellationToken, Task<TResource>> resourceFactoryAsync, Func<TResource, CancellationToken, Task<IObservable<TSource>>> observableFactoryAsync) where TResource : IDisposable
        {
            return Observable.FromAsync<TResource>(resourceFactoryAsync)
                .SelectMany(resource =>
                    Observable.Using<TSource, TResource>(
                        () => resource,
                        resource_ => Observable.FromAsync<IObservable<TSource>>(ct => observableFactoryAsync(resource_, ct)).Merge()
                    )
                );
        }

#endif

        #endregion
    }
}
