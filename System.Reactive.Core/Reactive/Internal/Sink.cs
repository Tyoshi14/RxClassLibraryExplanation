// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System.Threading;

namespace System.Reactive
{
    /// <summary>
    /// Base class for implementation of query operators, providing a lightweight sink that can be disposed to mute the outgoing observer.
    /// </summary>
    /// <typeparam name="TSource">Type of the resulting sequence's elements.</typeparam>
    /// <remarks>Implementations of sinks are responsible to enforce the message grammar on the associated observer. Upon sending a terminal message, a pairing Dispose call should be made to trigger cancellation of related resources and to mute the outgoing observer.</remarks>
    internal abstract class Sink<TSource> : IDisposable
    {
        protected internal volatile IObserver<TSource> _observer;
        private IDisposable _cancel;

        public Sink(IObserver<TSource> observer, IDisposable cancel)
        {
            _observer = observer;
            _cancel = cancel;
        }

        public virtual void Dispose()
        {
            _observer = NopObserver<TSource>.Instance;

            var cancel = Interlocked.Exchange(ref _cancel, null);
            if (cancel != null)
            {
                cancel.Dispose();
            }
        }

    // 返回一个观察着对象，观察对象里面封装了对IObserver接口的重载。
    // 不明白为什么要新建一个类 _ 来实现_observer 的三个功能。
        public IObserver<TSource> GetForwarder()
        {
            return new _(this);
        }

        class _ : IObserver<TSource>
        {
            private readonly Sink<TSource> _forward;

            public _(Sink<TSource> forward)
            {
                _forward = forward;
            }

            // Provides the observer with new data. 
            public void OnNext(TSource value)
            {
                _forward._observer.OnNext(value);
            }

            public void OnError(Exception error)
            {
                _forward._observer.OnError(error);
                _forward.Dispose();
            }

            public void OnCompleted()
            {
                _forward._observer.OnCompleted();
                _forward.Dispose();
            }
        }
    }
}
#endif