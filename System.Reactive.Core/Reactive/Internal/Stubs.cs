// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    // 由下面的代码可以看到Stub有两个类，不同点在于期中一个是泛型类，为什么要设计两个类？ 分析之后发现作用是不同的，Stubs定义了一个空类。

    internal static class Stubs<T>
    {
        //static readonly 与 const不同， nitialization can be at run time 
        // Note _  is a convention used when you don't care about the parameter.

        // Action 是一个系统代理类，“Encapsulates a method that has a single parameter and does not return a value.”。不明白微软为什么设计这么一个类。
        //Ignore 初始化一个空动作              --By Tyoshi
        public static readonly Action<T> Ignore = _ => { };
        // Func 系统代理类，“Encapsulates a method that has one parameter and returns a value of the type specified by the second parameter.”
        // I 不做任何变化，仍然保持原样。
        public static readonly Func<T, T> I = _ => _;
    }

    
    internal static class Stubs
    {
        //Nop 定义了一个空Action方法。
        //Throw 抛出一个错误信息。
        public static readonly Action Nop = () => { };
        public static readonly Action<Exception> Throw = ex => { ex.Throw(); };
    }

    //  !NO_THREAD 标签 是条件编译符
#if !NO_THREAD
    internal static class TimerStubs
    {
        public static readonly System.Threading.Timer Never = new System.Threading.Timer(_ => { });
    }
#endif
}
