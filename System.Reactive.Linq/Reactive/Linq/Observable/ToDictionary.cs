// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Base implement class of the Observable ToDictionary branch.
    /// </summary>
    class ToDictionary<TSource, TKey, TElement> : Producer<IDictionary<TKey, TElement>>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, TKey> _keySelector;
        private readonly Func<TSource, TElement> _elementSelector;
        private readonly IEqualityComparer<TKey> _comparer;

        public ToDictionary(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            _source = source;
            _keySelector = keySelector;
            _elementSelector = elementSelector;
            _comparer = comparer;
        }

        protected override IDisposable Run(IObserver<IDictionary<TKey, TElement>> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<IDictionary<TKey, TElement>>, IObserver<TSource>
        {
            private readonly ToDictionary<TSource, TKey, TElement> _parent;
            //Dictionary is a new data type.
            // Represents a collection of keys and values.
            private Dictionary<TKey, TElement> _dictionary;

            public _(ToDictionary<TSource, TKey, TElement> parent, IObserver<IDictionary<TKey, TElement>> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            // call the Dictionary constructor with a IEqualityComparer parameter.
            // Initialize an empty instance of System.Collections.Generic.Dictionary, and use the specified IEqualityComparer _comparer.
                _dictionary = new Dictionary<TKey, TElement>(_parent._comparer);
            }

            public void OnNext(TSource value)
            {
                try
                {
                    // call Dictionary.Add(key,value) function to add a new element to _dictionary.
                    // _keySelector is a function that convert a TSource data to a TKey type.
                    // _elementSelector is also a function which convert a TSource data to a TKey type.
                    // Watch  out！ Add  doesn't let to add a value a key already eixsts.
                    // System.ArgumentException: 
                    //           An element with the same key already exists in the System.Collections.Generic.Dictionary<TKey,TValue>.
                    _dictionary.Add(_parent._keySelector(value), _parent._elementSelector(value));
                }
                catch (Exception ex)
                {
                    base._observer.OnError(ex);
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
                base._observer.OnNext(_dictionary);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif