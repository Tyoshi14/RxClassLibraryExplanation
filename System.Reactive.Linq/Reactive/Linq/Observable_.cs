// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive.Linq
{
    /// <summary>
    /// Provides a set of static methods for writing in-memory queries over observable sequences.
    /// </summary>
    public static partial class Observable
    {
//s_impl来自这里。因此需要看看GetQueryImpl方法和QueryLanguage类。
//转了一圈其实s_impl应该就是个new QueryLanguage()
//然后试图找到QueryLanguage的定义发现好多文件
//原来是internal partial class QueryLanguage惹的祸
//根据直觉应该去看QueryLanguage.Aggregate.cs
        private static IQueryLanguage s_impl = QueryServices.GetQueryImpl<IQueryLanguage>(new QueryLanguage());
    }
}
