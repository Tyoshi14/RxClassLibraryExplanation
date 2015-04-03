using System;
using System.Collections.Generic;
using System.Reactive;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public static class 经验分布函数类
    {
        public static IObservable<IDictionary<随机变量值域, double>> ECDF<随机变量值域>(this IObservable<随机变量值域> source)
        {
            return new 经验分布函数类<随机变量值域>(source, Comparer<随机变量值域>.Default);
        }
        public static IObservable<IDictionary<随机变量值域, double>> ECDF<随机变量值域>(this IObservable<随机变量值域> source, IComparer<随机变量值域> 排序比较器)
        {
            return new 经验分布函数类<随机变量值域>(source, 排序比较器);
        }
    }
    public class 经验分布函数类<随机变量值域> : Producer<IDictionary<随机变量值域, double>>
    {
        private readonly IObservable<随机变量值域> _供应商可观察对象;
        private readonly IComparer<随机变量值域> _排序比较器;
        public 经验分布函数类(IObservable<随机变量值域> 供应商可观察对象, IComparer<随机变量值域> 排序比较器)
        {
            _供应商可观察对象 = 供应商可观察对象;
            _排序比较器 = 排序比较器;
        }
        protected override IDisposable Run(IObserver<IDictionary<随机变量值域, double>> 客户观察者, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new 内部处理器(_排序比较器, 客户观察者, cancel);
            setSink(sink);
            return _供应商可观察对象.SubscribeSafe(sink);
        }
        class 内部处理器 : Sink<IDictionary<随机变量值域, double>>, IObserver<随机变量值域>
        {
            private SortedDictionary<随机变量值域, int> _观测值频次统计表;
            private SortedDictionary<随机变量值域, int> _观测值累计频次统计表;
            private SortedDictionary<随机变量值域, double> _观测值累计概率统计表;
            public 内部处理器(IComparer<随机变量值域> 排序比较器, IObserver<IDictionary<随机变量值域, double>> 客户观察者, IDisposable cancel)
                : base(客户观察者, cancel)
            {
                _观测值频次统计表 = new SortedDictionary<随机变量值域, int>(排序比较器);
                _观测值累计频次统计表 = new SortedDictionary<随机变量值域, int>(排序比较器);
                _观测值累计概率统计表 = new SortedDictionary<随机变量值域, double>(排序比较器);
            }
            public void OnNext(随机变量值域 随机变量新观测值)
            {
                if(_观测值频次统计表.ContainsKey(随机变量新观测值))
                    _观测值频次统计表[随机变量新观测值]++;
                else
                    _观测值频次统计表[随机变量新观测值] = 1;
                int count = 0;
                foreach(var key in _观测值频次统计表.Keys)
                {
                    count += _观测值频次统计表[key];
                    _观测值累计频次统计表[key] = count;
                }
                foreach(var key in _观测值频次统计表.Keys)
                        _观测值累计概率统计表[key] = (double)_观测值累计频次统计表[key] / count;
                base._observer.OnNext(new Dictionary<随机变量值域, double>(_观测值累计概率统计表));
            }
            public void OnError(Exception error)
            {
                base._observer.OnError(error);
                base.Dispose();
            }
            public void OnCompleted()
            {
                base._observer.OnCompleted();
                base.Dispose();
            }
        }
    }
}
