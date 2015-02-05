// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Base implement class of the Observable ToLookup branch.
    /// 1.
    /// The code stucture is very similiar with ToDictionary branch. Then what is the difference between them ?
    ///  The difference is data structure. 
    ///     ToDictionary is just a dictionary with the  form Dictionary<K, E>.
    ///     ToLookup is based on dictionary and has a form of Dictionary<K, List<E>> with a list of E as its value.
    /// 2.
    /// The second question is why we have ToLookup and ToLookup at the same time ? 
    ///   However they are different. ToLookup allows you to have a group of data with the same key, and ToLookup sets a rule that a key only has one value.
    /// </summary>
    class ToLookup<TSource, TKey, TElement> : Producer<ILookup<TKey, TElement>>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, TKey> _keySelector;
        private readonly Func<TSource, TElement> _elementSelector;
        private readonly IEqualityComparer<TKey> _comparer;

        public ToLookup(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            _source = source;
            _keySelector = keySelector;
            _elementSelector = elementSelector;
            _comparer = comparer;
        }

        protected override IDisposable Run(IObserver<ILookup<TKey, TElement>> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(this, observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<ILookup<TKey, TElement>>, IObserver<TSource>
        {
            private readonly ToLookup<TSource, TKey, TElement> _parent;
            /// There we meet a new data type Lookup.
            /// Lookup privides a data structure that maps keys to  List<TElement> sequences of values.
            private Lookup<TKey, TElement> _lookup;

            public _(ToLookup<TSource, TKey, TElement> parent, IObserver<ILookup<TKey, TElement>> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
                _lookup = new Lookup<TKey, TElement>(_parent._comparer);
            }

            public void OnNext(TSource value)
            {
                try
                {
                    // call Lookup.Add(key,value) function to add a new element to _lookup.
                    // _keySelector is a function that convert a TSource data to a TKey type.
                    // _elementSelector is also a function which convert a TSource data to a TKey type.
                    _lookup.Add(_parent._keySelector(value), _parent._elementSelector(value));
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
                base._observer.OnNext(_lookup);
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif