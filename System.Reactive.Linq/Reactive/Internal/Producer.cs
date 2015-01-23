// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive
{
    /// <summary>
    /// Base class for implementation of query operators, providing performance benefits over the use of <see cref="System.Reactive.Linq.Observable.Create">Observable.Create</see>.
    /// </summary>
    /// <typeparam name="TSource">Type of the resulting sequence's elements.</typeparam>
    abstract class Producer<TSource> : IObservable<TSource>
    {
        /// <summary>
        /// Publicly visible Subscribe method.
        /// </summary>
        /// <param name="observer">Observer to send notifications on. The implementation of a producer must ensure the correct message grammar on the observer.</param>
        /// <returns>IDisposable to cancel the subscription. This causes the underlying sink to be notified of unsubscription, causing it to prevent further messages from being sent to the observer.</returns>
        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            // SingleAssignmentDisposable 是什么样的类？完成的是什么样的功能？
            //Class description： Represents a disposable resource which only allows a single assignment of its underlying disposable resource
            //  -By Tyoshi
            var sink = new SingleAssignmentDisposable();
            var subscription = new SingleAssignmentDisposable();

            // CurrentThreadScheduler 是一个线程控制类。
            // 类功能描述为： Represents an object that schedules units of work on the current thread.
            // ScheduleRequired 的描述为： Gets a value that indicates whether the caller must call a Schedule method.
            // Then what is Schedule method? Answer： 调度程序。
            // 但是最终都会调用Producer类中的 Run（） 方法。
            if (CurrentThreadScheduler.Instance.ScheduleRequired)
            {
                //如果需要调用Schedule()方法则调用  Description: Schedules an action to be executed after dueTime.
                CurrentThreadScheduler.Instance.Schedule(this, (_, me) => subscription.Disposable = me.Run(observer, subscription, s => sink.Disposable = s));
            }
            else
            {
                subscription.Disposable = this.Run(observer, subscription, s => sink.Disposable = s);
            }

            // CompositeDisposable类的作用是释放资源，该类的解释为：
            // Initializes a new instance of the class with the specified number of disposables.
            return new CompositeDisposable(2) { sink, subscription };
        }

        /// <summary>
        /// Core implementation of the query operator, called upon a new subscription to the producer object.
        /// </summary>
        /// <param name="observer">Observer to send notifications on. The implementation of a producer must ensure the correct message grammar on the observer.</param>
        /// <param name="cancel">The subscription disposable object returned from the Run call, passed in such that it can be forwarded to the sink, allowing it to dispose the subscription upon sending a final message (or prematurely for other reasons).</param>
        /// <param name="setSink">Callback to communicate the sink object to the subscriber, allowing consumers to tunnel a Dispose call into the sink, which can stop the processing.</param>
        /// <returns>Disposable representing all the resources and/or subscriptions the operator uses to process events.</returns>
        /// <remarks>The <paramref name="observer">observer</paramref> passed in to this method is not protected using auto-detach behavior upon an OnError or OnCompleted call. The implementation must ensure proper resource disposal and enforce the message grammar.</remarks>
        protected abstract IDisposable Run(IObserver<TSource> observer, IDisposable cancel, Action<IDisposable> setSink);
    }
}
#endif