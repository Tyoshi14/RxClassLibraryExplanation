// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
   /// <summary>
   ///  Represents an observable that can be evaluate.
   /// </summary>
   /// <typeparam name="T"></typeparam>
    interface IEvaluatableObservable<T>
    {
        IObservable<T> Eval();
    }
}
