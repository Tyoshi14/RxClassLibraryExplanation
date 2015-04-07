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
    //TO-DO
    //随机变量值域 现在是一维的，需要扩展为多维，且每一维度的类型可以不同
    //例如：随机变量值域:Tuple<DateTime,string,TimeSpan,bool>
    //注意到，随机变量值域 不论哪个维度都不必是数值类型，只要可以排序即可
    public class 经验分布函数类<随机变量值域> : Producer<IDictionary<随机变量值域, double>>
    {
        private readonly IObservable<随机变量值域> _供应商可观察对象;
        private readonly IComparer<随机变量值域> _排序比较器;
        private 内部处理器 _内部处理器;
        public 经验分布函数类(IObservable<随机变量值域> 供应商可观察对象, IComparer<随机变量值域> 排序比较器)
        {
            _供应商可观察对象 = 供应商可观察对象;
            _排序比较器 = 排序比较器;
        }
        protected override IDisposable Run(IObserver<IDictionary<随机变量值域, double>> 客户观察者, IDisposable cancel, Action<IDisposable> setSink)
        {
            _内部处理器 = new 内部处理器(_排序比较器, 客户观察者, cancel);
            setSink(_内部处理器);
            return _供应商可观察对象.SubscribeSafe(_内部处理器);
        }
        //TO-DO:
        /// <summary>
        /// 不改变分布的情况下将额外的查询数据流转换为累计概率流
        /// </summary>
        /// <param name="source">查询数据流</param>
        /// <returns>累计概率数据流</returns>
        public IObservable<double> CDF(IObservable<随机变量值域> source)
        {
            throw new NotImplementedException();
        }

        //TO-DO:
        /// <summary>
        /// 不改变分布的情况下将额外的查询数据流转换为分位数值流
        /// </summary>
        /// <param name="source">查询数据流</param>
        /// <returns>分位数值流</returns>
        public IObservable<随机变量值域> ICDF(IObservable<double> source)
        {
            throw new NotImplementedException();
        }

        class 内部处理器 : Sink<IDictionary<随机变量值域, double>>, IObserver<随机变量值域>
        {
            private int _总样本数;
            private SortedDictionary<随机变量值域, int> _观测值频次统计表;
            private SortedDictionary<随机变量值域, double> _观测值累计概率统计表;
            public 内部处理器(IComparer<随机变量值域> 排序比较器, IObserver<IDictionary<随机变量值域, double>> 客户观察者, IDisposable cancel)
                : base(客户观察者, cancel)
            {
                _总样本数 = 0;
                _观测值频次统计表 = new SortedDictionary<随机变量值域, int>(排序比较器);
                _观测值累计概率统计表 = new SortedDictionary<随机变量值域, double>(排序比较器);
            }
            //TO-DO:
            //改进数据结构和算法。目前采用.NET内置的红黑树，即SortedDictionary
            //每次OnNext被调用都要处理Nlog(N)复杂性，其中N是已经存在的随机变量不同取值数
            public void OnNext(随机变量值域 随机变量新观测值)
            {
               
                _总样本数++;
                if(_观测值频次统计表.ContainsKey(随机变量新观测值))
                    _观测值频次统计表[随机变量新观测值]++;
                else
                    _观测值频次统计表[随机变量新观测值] = 1;
                int count = 0;
                foreach(var key in _观测值频次统计表.Keys)
                {
                    count += _观测值频次统计表[key];
                    _观测值累计概率统计表[key] = (double)count / _总样本数;
                }

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
