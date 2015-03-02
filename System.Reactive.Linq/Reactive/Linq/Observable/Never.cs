// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl
{
    /// <summary>
    /// Base implement class of the Observable Never branch.
    ///  Returns a non-terminating observable sequence. which can be used to denote an infinite duration
    /// </summary>
    class Never<TResult> : IObservable<TResult>
    {
        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            /// Gets the disposable that does nothing when disposed.
            return Disposable.Empty;
        }
    }
}
#endif