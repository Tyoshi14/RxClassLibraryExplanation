using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public static class SearchEF
    {
        // Seach for probility
        public static IObservable<double> CDF<随机变量值域>(this IObservable<随机变量值域> searchSource, IObservable<IDictionary<随机变量值域, double>> source)
        {
            return new CDF<随机变量值域>(source, searchSource, Comparer<随机变量值域>.Default);
        }

        public static IObservable<double> CDF<随机变量值域>(this IObservable<随机变量值域> searchSource, IDictionary<随机变量值域, double> source)
        {
            return new CDF<随机变量值域>(source, searchSource, Comparer<随机变量值域>.Default);
        }

        // Search objects according to probility
        public static IObservable<随机变量值域> ICDF<随机变量值域>(this IObservable<double> searchSource, IDictionary<随机变量值域, double> dict)
        {
            return new ICDF<随机变量值域>(searchSource, dict, Comparer<double>.Default);
        }

        public static IObservable<随机变量值域> ICDF<随机变量值域>(this IObservable<double> searchSource, IObservable<IDictionary<随机变量值域, double>> dictSource)
        {
            return new ICDF<随机变量值域>(searchSource, dictSource, Comparer<double>.Default);
        }

    }

    public class CDF<随机变量值域> : Producer<double>
    {

        private readonly IObservable<随机变量值域> _searchSource;
        private volatile IDictionary<随机变量值域, double> _dict;
        private readonly IComparer<随机变量值域> _comparer;

        public CDF(IObservable<IDictionary<随机变量值域, double>> dictSource, IObservable<随机变量值域> searchSource, IComparer<随机变量值域> comparer)
        {

            _searchSource = searchSource;
            //需要订阅并不断更新最新数据表而不是获得一个静态的。
            // There we can get the latest dictionary!
            dictSource.Subscribe((dict) => _dict = dict);
            _comparer = comparer;
        }

        public CDF(IDictionary<随机变量值域, double> dict, IObservable<随机变量值域> searchSource, IComparer<随机变量值域> comparer)
        {
            _dict = dict;
            _searchSource = searchSource;
            _comparer = comparer;
        }

        protected override IDisposable Run(IObserver<double> observer, IDisposable cancel, Action<IDisposable> setSink)
        {

            var sink = new CDF_sink(observer, cancel, _dict, _comparer);
            setSink(sink);
            return _searchSource.SubscribeSafe(sink);
        }



        class CDF_sink : Sink<double>, IObserver<随机变量值域>
        {
            private IDictionary<随机变量值域, double> dictionary;
            private IComparer<随机变量值域> comparer;
            public CDF_sink(IObserver<double> observer, IDisposable cancel, IDictionary<随机变量值域, double> dict, IComparer<随机变量值域> comp)
                : base(observer, cancel)
            {
                dictionary = dict;
                comparer = comp;
                foreach (var kv in dict)
                {
                    Console.WriteLine("{0},{1}", kv.Key, kv.Value);
                }
            }


            public void OnNext(随机变量值域 value)
            {

                var probility = getProbility(value);
                base._observer.OnNext(probility);

            }

            /// <summary>
            /// Get the probility of the certain value.
            /// </summary>
            private double getProbility(随机变量值域 value)
            {
                double result = default(double);
                随机变量值域 fommer = default(随机变量值域);
                随机变量值域 latter = default(随机变量值域);

                if (dictionary.Keys.Contains(value))
                {
                    dictionary.TryGetValue(value, out result);
                    //Console.WriteLine("Contains value {0} result {1}", value, result);
                    return result;
                }

                foreach(var item in dictionary.Keys)
                {
                    if (dictionary.Last().Key.Equals(item))
                    {
                        return 1.0;
                    }

                    if(comparer.Compare(item, value) > 0)
                    {
                        latter = item;
                        break;
                    }
                    fommer = item;
                }
                result = calculateProbability(fommer, latter, value);
              
                return result;
            }

            private double calculateProbability(随机变量值域 former, 随机变量值域 latter, 随机变量值域 value)
            {
                var _former = default(double);
                var _latter = default(double);
                var result = default(double);

                dictionary.TryGetValue(former, out _former);
                dictionary.TryGetValue(latter, out _latter);
              
                if (value is int|| value is double|| value is float || value is decimal)
                {
                    double _fObject = Convert.ToDouble(former);
                    double _LObject = Convert.ToDouble(latter);
                    double k = (double)((_latter - _former) / (_LObject - _fObject));
                    double b = _latter - k * _LObject;
                    result = k * Convert.ToDouble(value) + b;
                    //Console.WriteLine("value {0} K {1}, b {2} result {3}",value,k,b,result);
                }
                else {
                    result =(double) ((_former + _latter) / 2);
                    //Console.WriteLine("value {0} result {1}",value, result);
                }


                return result;
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




    public class ICDF<随机变量值域> : Producer<随机变量值域>
    {

        private readonly IObservable<double> _searchSource;
        private readonly IComparer<double> _comparer;
        private volatile IDictionary<随机变量值域, double> _dict;

        public ICDF(IObservable<double> searchSource, IDictionary<随机变量值域, double> dict, IComparer<double> comparer)
        {
            _searchSource = searchSource;
            _dict = dict;
            _comparer = comparer;

        }

        public ICDF(IObservable<double> searchSource, IObservable<IDictionary<随机变量值域, double>> dictSource, IComparer<double> comparer)
        {
            _searchSource = searchSource;
            _comparer = comparer;
            dictSource.Subscribe((dict) => _dict = dict);

        }
        protected override IDisposable Run(IObserver<随机变量值域> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new ICDF_sink(observer, cancel, _dict, _comparer);
            setSink(sink);
            return _searchSource.SubscribeSafe(sink);
        }

        class ICDF_sink : Sink<随机变量值域>, IObserver<double>
        {
            private IDictionary<随机变量值域, double> _dictionary;
            private readonly IComparer<double> _comparer;

            public ICDF_sink(IObserver<随机变量值域> observer, IDisposable cancel, IDictionary<随机变量值域, double> dict, IComparer<double> comparer)
                : base(observer, cancel)
            {
                _dictionary = dict;
                _comparer = comparer;
            }

            public void OnNext(double value)
            {

                var keyValue = getICDFvalue(value);
                base._observer.OnNext(keyValue);

            }

            // The core code of ICDF 
            // In this part we can get certain 随机变量值域 value according to the probility sequence.
            private 随机变量值域 getICDFvalue(double value)
            {
                // There we apply some algorithm to implement the search function.
                // The principle there is that we return the last object  whose probility  is smaller than the target object value.
                // Otherwise we return the default values.
                var returnvalue = default(随机变量值域);
                var probility = default(double);
                foreach(var item in _dictionary.Keys)
                {
                    _dictionary.TryGetValue(item, out probility);
                    if(_comparer.Compare(value, probility) <= 0)
                    {
                        returnvalue = item;
                    }
                }
                return returnvalue;
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

