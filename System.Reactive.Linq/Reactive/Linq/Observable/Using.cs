// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl
{
    class Using<TSource, TResource> : Producer<TSource>
        where TResource : IDisposable
    {
        private readonly Func<TResource> _resourceFactory;
        private readonly Func<TResource, IObservable<TSource>> _observableFactory;

        public Using(Func<TResource> resourceFactory, Func<TResource, IObservable<TSource>> observableFactory)
        {
            _resourceFactory = resourceFactory;
            _observableFactory = observableFactory;
        }

        protected override IDisposable Run(IObserver<TSource> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            return sink.Run();
        }

        class _ : Sink<TSource>, IObserver<TSource>
        {
            private readonly Using<TSource, TResource> _parent;

            public _(Using<TSource, TResource> parent, IObserver<TSource> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            /// <summary>
            /// The core code in Using.
            /// 
            /// Here comes its logic.
            ///     Get the resource object called resource
            ///     IF resource ne null
            ///         Generate an element using factory function _observableFactory
            ///  return source.subsribe()
            ///  
            /// </summary>
            /// <returns></returns>
            public IDisposable Run()
            {
                var source = default(IObservable<TSource>);
                var disposable = Disposable.Empty;
                try
                {
                    var resource = _parent._resourceFactory();
                    if (resource != null)
                        disposable = resource; /// (1)
                    source = _parent._observableFactory(resource);
                }
                catch (Exception exception)
                {
                    return new CompositeDisposable(Observable.Throw<TSource>(exception).SubscribeSafe(this), disposable);
                }

                // Form (1) and (2) we can see that the lifetime of resouce object se lifetime is tied to the resulting observable sequence's lifetime.
                return new CompositeDisposable(source.SubscribeSafe(this), disposable);///(2)
            }

            public void OnNext(TSource value)
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