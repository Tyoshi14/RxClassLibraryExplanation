// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Reflection;

namespace System.Reactive.PlatformServices
{
    /// <summary>
    /// (Infrastructure) Interface for enlightenment providers.
    /// </summary>
    /// <remarks>
    /// This type is used by the Rx infrastructure and not meant for public consumption or implementation.
    /// No guarantees are made about forward compatibility of the type's functionality and its usage.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IPlatformEnlightenmentProvider
    {
        /// <summary>
        /// (Infastructure) Tries to gets the specified service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="args">Optional set of arguments.</param>
        /// <returns>Service instance or null if not found.</returns>
        T GetService<T>(params object[] args) where T : class;
    }

    /// <summary>
    /// (Infrastructure) Provider for platform-specific framework enlightenments.
    /// </summary>
    /// <remarks>
    /// This type is used by the Rx infrastructure and not meant for public consumption or implementation.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PlatformEnlightenmentProvider
    {
        private static readonly object s_gate = new object();
        private static IPlatformEnlightenmentProvider s_current;

        /// <summary>
        /// (Infrastructure) Gets the current enlightenment provider. If none is loaded yet, accessing this property triggers provider resolution.
        /// </summary>
        /// <remarks>
        /// This member is used by the Rx infrastructure and not meant for public consumption or implementation.
        /// </remarks>
//首先发现Current在此
        public static IPlatformEnlightenmentProvider Current
        {
            get
            {
                if (s_current == null)
                {
                    lock (s_gate)
                    {
                        if (s_current == null)
                        {
                            //
                            // TODO: Investigate whether we can simplify this logic to just use "System.Reactive.PlatformServices.PlatformEnlightenmentProvider, System.Reactive.PlatformServices".
                            //       It turns out this doesn't quite work on Silverlight. On the other hand, in .NET Compact Framework 3.5, we mysteriously have to use that path.
                            //

#if NETCF35
                            var name = "System.Reactive.PlatformServices.CurrentPlatformEnlightenmentProvider, System.Reactive.PlatformServices";
#else
#if CRIPPLED_REFLECTION && HAS_WINRT
                            var ifType = typeof(IPlatformEnlightenmentProvider).GetTypeInfo();
#else
                            var ifType = typeof(IPlatformEnlightenmentProvider);
#endif
                            var asm = new AssemblyName(ifType.Assembly.FullName);
                            asm.Name = "System.Reactive.PlatformServices";
                            var name = "System.Reactive.PlatformServices.CurrentPlatformEnlightenmentProvider, " + asm.FullName;
#endif
//第四步发现要判断name啥啥的
                            var t = Type.GetType(name, false);
                            if (t != null)
                                s_current = (IPlatformEnlightenmentProvider)Activator.CreateInstance(t);
                            else
                                s_current = new DefaultPlatformEnlightenmentProvider();
                        }
                    }
                }
//然后发现优先返回s_current的逻辑
                return s_current;
            }
//第三步看到s_current只有set可以初始化
            set
            {
                lock (s_gate)
                {
                    s_current = value;
                }
            }
        }
    }

    class DefaultPlatformEnlightenmentProvider : IPlatformEnlightenmentProvider
    {
        public T GetService<T>(object[] args) where T : class
        {
//第五步意识到DefaultPlatformEnlightenmentProvider的GetService会返回null从而使得这一支没有价值
//加上脑力不足（Stack overflow）回到上一级（pop）
            return null;
        }
    }
}
