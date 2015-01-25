// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive
{
    /// <summary>
    /// Abstract base class for implementations of the IObservable&lt;T&gt; interface.
    /// </summary>
    /// <remarks>
    /// If you don't need a named type to create an observable sequence (i.e. you rather need
    /// an instance rather than a reusable type), use the Observable.Create method to create
    /// an observable sequence with specified subscription behavior.
    /// </remarks>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    public abstract class ObservableBase<T> : IObservable<T>
    {
        /// <summary>
        /// Subscribes the given observer to the observable sequence.
        /// </summary>
        /// <param name="observer">Observer that will receive notifications from the observable sequence.</param>
        /// <returns>Disposable object representing an observer's subscription to the observable sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="observer"/> is null.</exception>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            // AutoDetachObserver类是一个non-stopped的observer对象。
            var autoDetachObserver = new AutoDetachObserver<T>(observer);
            
            // 在对资源进行订阅是，仍然分为两种情况，一种是利用调度函数调度函数执行，另一种是直接执行。 目前见到的所有Subscribe方法都是这种实现方式，需要搞清原因。
            // --- Question remained by Tyoshi
            if (CurrentThreadScheduler.IsScheduleRequired)
            {
                //
                // Notice we don't protect this piece of code using an exception handler to
                // redirect errors to the OnError channel. This call to Schedule will run the
                // trampoline, so we'd be catching all exceptions, including those from user
                // callbacks that happen to run there. For example, consider:
                //
                //    Observable.Return(42, Scheduler.CurrentThread)
                //              .Subscribe(x => { throw new Exception(); });
                //
                // Here, the OnNext(42) call would be scheduled on the trampoline, so when we
                // return from the scheduled Subscribe call, the CurrentThreadScheduler moves
                // on to invoking this work item. Too much of protection here would cause the
                // exception thrown in OnNext to circle back to OnError, which looks like the
                // sequence can't make up its mind.
                // param1 是一个Observer类型， param2 是一个函数。返回的是 IDisposable 资源释放函数类型。
                CurrentThreadScheduler.Instance.Schedule(autoDetachObserver, ScheduledSubscribe);
            }
            else
            {
                try
                {
                    autoDetachObserver.Disposable = SubscribeCore(autoDetachObserver);
                }
                catch (Exception exception)
                {
                    //
                    // This can happen when there's a synchronous callback to OnError in the
                    // implementation of SubscribeCore, which also throws. So, we're seeing
                    // an exception being thrown from a handler.
                    //
                    // For compat with v1.x, we rethrow the exception in this case, keeping
                    // in mind this should be rare but if it happens, something's totally
                    // screwed up.
                    //
                    if (!autoDetachObserver.Fail(exception))
                        throw;
                }
            }

            return autoDetachObserver;
        }

        // Subscribe方法调度函数分支调用的订阅函数实现。
        // ScheduledSubscribe()含有IScheduler _ 变量，但是函数实现中根本就没有用到，难道只是为了满足某些需要调度线程的需求增加的一个幌子？？
        // --- Question remained by Tyoshi
        private IDisposable ScheduledSubscribe(IScheduler _, AutoDetachObserver<T> autoDetachObserver)
        {
            try
            {
        // 最终还是调用的订阅函数的非调度实现分支。
                autoDetachObserver.Disposable = SubscribeCore(autoDetachObserver);
            }
            catch (Exception exception)
            {
                //
                // This can happen when there's a synchronous callback to OnError in the
                // implementation of SubscribeCore, which also throws. So, we're seeing
                // an exception being thrown from a handler.
                //
                // For compat with v1.x, we rethrow the exception in this case, keeping
                // in mind this should be rare but if it happens, something's totally
                // screwed up.
                //
                if (!autoDetachObserver.Fail(exception))
                    throw;
            }

            return Disposable.Empty;
        }

        /// <summary>
        /// Implement this method with the core subscription logic for the observable sequence. 抽象方法也是核心方法，其继承接口必须要实现。
        /// </summary>
        /// <param name="observer">Observer to send notifications to.</param>
        /// <returns>Disposable object representing an observer's subscription to the observable sequence.</returns>
        protected abstract IDisposable SubscribeCore(IObserver<T> observer);
    }
}
