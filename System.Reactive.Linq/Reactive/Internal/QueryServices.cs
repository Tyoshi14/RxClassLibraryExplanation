// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reactive.PlatformServices;

namespace System.Reactive.Linq
{
    internal static class QueryServices
    {
        private static Lazy<IQueryServices> s_services = new Lazy<IQueryServices>(Initialize);

//GetQueryImpl来自这里。本质是Lazy<IQueryServices>.Value.Extend。这里先去看看Lazy是个什么容器。
//Lazy<T>.Value是System.Lazy<T>系统自带的属性其解释是
//Gets the lazily initialized value of the current System.Lazy<T> instance.
//Lazy<T>和Nullable<T>可以类比这学，可以认为是对T Value的一个包装。自己回去看就好了不是阅读Rx的重点。
        public static T GetQueryImpl<T>(T defaultInstance)
        {
            return s_services.Value.Extend(defaultInstance);
        }

        private static IQueryServices Initialize()
        {
            return PlatformEnlightenmentProvider.Current.GetService<IQueryServices>() ?? new DefaultQueryServices();
        }
    }

    internal interface IQueryServices
    {
        T Extend<T>(T baseImpl);
    }

    class DefaultQueryServices : IQueryServices
    {
        public T Extend<T>(T baseImpl)
        {
            return baseImpl;
        }
    }
}
