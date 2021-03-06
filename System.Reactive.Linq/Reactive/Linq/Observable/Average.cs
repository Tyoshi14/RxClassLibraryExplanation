﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_PERF
using System;

namespace System.Reactive.Linq.ObservableImpl
{
  
    /// <summary> Need  to  finish!
    /// 张老师提出的关于 Aveage 实现的需要改进的地方： 
    ///上面的平均数只有在数据完结后才给出结果
    ///移动平均在首次填满移动窗口后就要开始输出结果了
    //因此需要把里程碑1所要求的多个文件中涉及到的方法进行分析寻找灵感
    /// </summary>

    class AverageDouble : Producer<double>
    {
        private readonly IObservable<double> _source;
//第一步发现初始化函数就是存了一下来源
        public AverageDouble(IObservable<double> source)
        {
            _source = source;
        }
//第二步发现没有IObservable所需的Subscribe
//但下面这个方法既有Subscribe所需的参数IObserver<double> observer
//又能返回Subscribe所需的返回值IDisposable
        protected override IDisposable Run(IObserver<double> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
//第三步看下面的第一行和第三行可以发现
//这个相当于return _source.SubscribeSafe(new _(observer, cancel))
//含义上就是用神秘下划线对象桥接了数据源_source和数据用户的用户observer
//                     note： 张老师的专业说法，值得学习！ 我在 All 类底层实现中添加了自己对这一过程的代码调用的理解。
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<double>, IObserver<double>
        {
            private double _sum;
            private long _count;
//第四步神秘类的初始化函数和算平均值的初始化如出一辙
            public _(IObserver<double> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0.0;
                _count = 0L;
            }
//第五步神秘类的OnNext响应和算平均值的循环体如出一辙
            public void OnNext(double value)
            {
                try
                {
                    checked
                    {
                        _sum += value;
                        _count++;
                    }
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
                if (_count > 0)
                {
//第六步神秘类的OnCompleted不就是汇报一下最终结果嘛
                    base._observer.OnNext(_sum / _count);
                    base._observer.OnCompleted();
                }
                else
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                }

                base.Dispose();
            }
        }
    }
//第七步对于double的Average实现看完

    class AverageSingle : Producer<float>
    {
        private readonly IObservable<float> _source;

        public AverageSingle(IObservable<float> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<float> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<float>, IObserver<float>
        {
            // 关于C# 的数据类型，请参考下面的网址。
            // http://www.tutorialspoint.com/csharp/csharp_data_types.htm 
            private double _sum; // NOTE: Uses a different accumulator type (double), conform LINQ to Objects.
            private long _count;

            public _(IObserver<float> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0.0;
                _count = 0L;
            }

            public void OnNext(float value)
            {
                try
                {
                //Key words:
                //The checked keyword is used to explicitly enable overflow checking for integral-type arithmetic operations and conversions.
                    checked
                    {
                        _sum += value;
                        _count++;
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext((float)(_sum / _count));
                    base._observer.OnCompleted();
                }
                else
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                }

                base.Dispose();
            }
        }
    }

 // decimal 数据类型的 Average（） 函数实现。
    class AverageDecimal : Producer<decimal>
    {
        private readonly IObservable<decimal> _source;

        public AverageDecimal(IObservable<decimal> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<decimal> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<decimal>, IObserver<decimal>
        {
            private decimal _sum;
            private long _count;

            public _(IObserver<decimal> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0M;
                _count = 0L;
            }

            public void OnNext(decimal value)
            {
                try
                {
                    checked
                    {
                        _sum += value;
                        _count++;
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext(_sum / _count);
                    base._observer.OnCompleted();
                }
                else
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                }

                base.Dispose();
            }
        }
    }

// AverageInt32 与 AverageInt64 实现一致。
    class AverageInt32 : Producer<double>
    {
        private readonly IObservable<int> _source;

        public AverageInt32(IObservable<int> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<double> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<double>, IObserver<int>
        {
            private long _sum;
            private long _count;

            public _(IObserver<double> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0L;
                _count = 0L;
            }

            public void OnNext(int value)
            {
                try
                {
                    checked
                    {
                        _sum += value;
                        _count++;
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext((double)_sum / _count);
                    base._observer.OnCompleted();
                }
                else
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                }

                base.Dispose();
            }
        }
    }

    class AverageInt64 : Producer<double>
    {
        private readonly IObservable<long> _source;

        public AverageInt64(IObservable<long> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<double> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<double>, IObserver<long>
        {
            private long _sum;
            private long _count;

            public _(IObserver<double> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0L;
                _count = 0L;
            }

            public void OnNext(long value)
            {
                try
                {
                    checked
                    {
                        _sum += value;
                        _count++;
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext((double)_sum / _count);
                    base._observer.OnCompleted();
                }
                else
                {
                    base._observer.OnError(new InvalidOperationException(Strings_Linq.NO_ELEMENTS));
                }

                base.Dispose();
            }
        }
    }

    class AverageDoubleNullable : Producer<double?>
    {
        private readonly IObservable<double?> _source;

        public AverageDoubleNullable(IObservable<double?> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<double?> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

 //新类型 double? 代表什么？
// 表示观察序列包含的元素为double 或者 null. 
        class _ : Sink<double?>, IObserver<double?>
        {
            private double _sum;
            private long _count;

            public _(IObserver<double?> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0.0;
                _count = 0L;
            }

            public void OnNext(double? value)
            {
                try
                {
                    // 这里可以看出，结果的计算只包含非空的变量。 
                    checked
                    {
                        if (value != null)
                        {
                            _sum += value.Value;
                            _count++;
                        }
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext(_sum / _count);
                }
                else
                {
                    base._observer.OnNext(null);
                }

                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }

    class AverageSingleNullable : Producer<float?>
    {
        private readonly IObservable<float?> _source;

        public AverageSingleNullable(IObservable<float?> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<float?> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<float?>, IObserver<float?>
        {
            private double _sum; // NOTE: Uses a different accumulator type (double), conform LINQ to Objects.
            private long _count;

            public _(IObserver<float?> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0.0;
                _count = 0L;
            }

            public void OnNext(float? value)
            {
                try
                {
                    checked
                    {
                        if (value != null)
                        {
                            _sum += value.Value;
                            _count++;
                        }
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext((float)(_sum / _count));
                }
                else
                {
                    base._observer.OnNext(null);
                }

                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }

    class AverageDecimalNullable : Producer<decimal?>
    {
        private readonly IObservable<decimal?> _source;

        public AverageDecimalNullable(IObservable<decimal?> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<decimal?> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<decimal?>, IObserver<decimal?>
        {
            private decimal _sum;
            private long _count;

            public _(IObserver<decimal?> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0M;
                _count = 0L;
            }

            public void OnNext(decimal? value)
            {
                try
                {
                    checked
                    {
                        if (value != null)
                        {
                            _sum += value.Value;
                            _count++;
                        }
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext(_sum / _count);
                }
                else
                {
                    base._observer.OnNext(null);
                }

                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }

    class AverageInt32Nullable : Producer<double?>
    {
        private readonly IObservable<int?> _source;

        public AverageInt32Nullable(IObservable<int?> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<double?> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<double?>, IObserver<int?>
        {
            private long _sum;
            private long _count;

            public _(IObserver<double?> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0L;
                _count = 0L;
            }

            public void OnNext(int? value)
            {
                try
                {
                    checked
                    {
                        if (value != null)
                        {
                            _sum += value.Value;
                            _count++;
                        }
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext((double)_sum / _count);
                }
                else
                {
                    base._observer.OnNext(null);
                }

                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }

    class AverageInt64Nullable : Producer<double?>
    {
        private readonly IObservable<long?> _source;

        public AverageInt64Nullable(IObservable<long?> source)
        {
            _source = source;
        }

        protected override IDisposable Run(IObserver<double?> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new _(observer, cancel);
            setSink(sink);
            return _source.SubscribeSafe(sink);
        }

        class _ : Sink<double?>, IObserver<long?>
        {
            private long _sum;
            private long _count;

            public _(IObserver<double?> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _sum = 0L;
                _count = 0L;
            }

            public void OnNext(long? value)
            {
                try
                {
                    checked
                    {
                        if (value != null)
                        {
                            _sum += value.Value;
                            _count++;
                        }
                    }
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
                if (_count > 0)
                {
                    base._observer.OnNext((double)_sum / _count);
                }
                else
                {
                    base._observer.OnNext(null);
                }

                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
#endif
