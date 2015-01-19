// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reactive.PlatformServices;

namespace System.Reactive.Linq
{
    internal static class QueryServices
    {
//而s_services来自Initialize
        private static Lazy<IQueryServices> s_services = new Lazy<IQueryServices>(Initialize);

//GetQueryImpl来自这里。本质是Lazy<IQueryServices>.Value.Extend。这里先去看看Lazy是个什么容器。
//Lazy<T>.Value是System.Lazy<T>系统自带的属性其解释是
//Gets the lazily initialized value of the current System.Lazy<T> instance.
//Lazy<T>和Nullable<T>可以类比这学，可以认为是对T Value的一个包装。自己回去看就好了不是阅读Rx的重点。
//下面看Extend是怎么回事。
//Extend的实现来自s_services。
        public static T GetQueryImpl<T>(T defaultInstance)
        {
            return s_services.Value.Extend(defaultInstance);
        }

        private static IQueryServices Initialize()
        {
//最后发现s_services.Value来自下面的返回值。那就得研究两种情形。
//分别是PlatformEnlightenmentProvider.Current.GetService<IQueryServices>()和new DefaultQueryServices()
//事实上，如果能够调试的话也许就知道是那种情形了。
            return PlatformEnlightenmentProvider.Current.GetService<IQueryServices>() ?? new DefaultQueryServices();
        }
    }

    internal interface IQueryServices
    {
//Extend的接口定义在此。那Lazy<IQueryServices>.Value.Extend用到的实现呢？
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
