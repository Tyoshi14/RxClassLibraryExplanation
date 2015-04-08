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
    public  static class  SearchEF
    {
        // Seach for probility
        public static IObservable<TResult> CDF<随机变量值域, TResult>(this IObservable<随机变量值域> searchSource, IObservable<IDictionary<随机变量值域, TResult>> source)
        {
            return new CDF<随机变量值域, TResult>(source, searchSource, Comparer<随机变量值域>.Default);
        }

        public static IObservable<TResult> CDF<随机变量值域, TResult>(this IObservable<随机变量值域> searchSource, IDictionary<随机变量值域, TResult> source) {
            return new CDF<随机变量值域, TResult>(source, searchSource, Comparer<随机变量值域>.Default);
        }

        // Search objects according to probility
        public static IObservable<随机变量值域> ICDF<随机变量值域, TResult>(this IObservable<TResult> searchSource, IDictionary<随机变量值域, TResult> dict)
        {
            return new ICDF<随机变量值域, TResult>(searchSource, dict, Comparer<TResult>.Default);
        }

        public static IObservable<随机变量值域> ICDF<随机变量值域, TResult>(this IObservable<TResult> searchSource, IObservable<IDictionary<随机变量值域, TResult>> dictSource)
        {
            return new ICDF<随机变量值域, TResult>(searchSource, dictSource,Comparer<TResult>.Default);
        }

   }

    public class CDF<随机变量值域, TResult> : Producer<TResult>
    {
       
        private readonly IObservable<随机变量值域> _searchSource;
        private volatile IDictionary<随机变量值域, TResult> _dict;
        private readonly IComparer<随机变量值域> _comparer;

        public CDF(IObservable<IDictionary<随机变量值域, TResult>> dictSource, IObservable<随机变量值域> searchSource, IComparer<随机变量值域> comparer)
        {
           
            _searchSource = searchSource;
            //需要订阅并不断更新最新数据表而不是获得一个静态的。
            // There we can get the latest dictionary!
            dictSource.Subscribe((dict) => _dict = dict);
            _comparer = comparer;
        }

        public CDF(IDictionary<随机变量值域, TResult> dict, IObservable<随机变量值域> searchSource,IComparer<随机变量值域> comparer)
        {
            _dict = dict;
            _searchSource = searchSource;
            _comparer = comparer;
        }
       
        protected override IDisposable Run(IObserver<TResult> observer, IDisposable cancel, Action<IDisposable> setSink)
        {

            var sink = new CDF_sink(observer, cancel, _dict, _comparer);
            setSink(sink);
            return _searchSource.SubscribeSafe(sink);
        }



        class CDF_sink : Sink<TResult>, IObserver<随机变量值域>
        {
            private IDictionary<随机变量值域, TResult> dictionary;
            private IComparer<随机变量值域> comparer;
            public CDF_sink(IObserver<TResult> observer, IDisposable cancel, IDictionary<随机变量值域, TResult> dict,IComparer<随机变量值域> comp)
                : base(observer, cancel)
            {
                dictionary = dict;
                comparer = comp;
            }

            
            public void OnNext(随机变量值域 value)
            {

                var probility = getProbility(value);
                base._observer.OnNext(probility);

            }

            /// <summary>
            /// Get the probility of the certain value.
            /// </summary>
            private TResult getProbility(随机变量值域 value)
            {
                TResult result = default(TResult);
                随机变量值域 fommer = default(随机变量值域);
                随机变量值域 latter = default(随机变量值域);
                foreach (var item in dictionary.Keys)
                {
                    if (dictionary.Keys.Contains(item))
                    {
                        dictionary.TryGetValue(item,out result);
                        return result;
                    }

                    if (comparer.Compare(item, value) > 0)
                    {
                        latter = item;
                        break;
                    }
                    fommer = item;
                }

                result = calculateProbability(fommer, latter, value);
                return result;
            }

            private TResult calculateProbability(随机变量值域 former,随机变量值域 latter,随机变量值域 value) 
            {
                var _former = default(TResult);
                var _latter = default(TResult);
                dictionary.TryGetValue(former,out _former);
                dictionary.TryGetValue(latter, out _latter);

              // There  I have some problems. For the reason that I cant convert TResult and another type to types that can calculate, for example Int double or float and so on!!!
              // So how should I coculate the Probility???
              // Now I only return the former object which is smaller than the passed parameter value!!!
                return _former;
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




    public class ICDF<随机变量值域, TResult> : Producer<随机变量值域> {
       
        private readonly IObservable<TResult> _searchSource;
        private readonly IComparer<TResult> _comparer;
        private volatile IDictionary<随机变量值域, TResult> _dict;

        public ICDF(IObservable<TResult> searchSource, IDictionary<随机变量值域, TResult> dict, IComparer<TResult> comparer)
        {
            _searchSource = searchSource;
            _dict = dict;
            _comparer = comparer;
            
        }

        public ICDF(IObservable<TResult> searchSource, IObservable<IDictionary<随机变量值域, TResult>> dictSource,IComparer<TResult> comparer)
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

        class ICDF_sink : Sink<随机变量值域>, IObserver<TResult> 
        {
            private IDictionary<随机变量值域, TResult> _dictionary;
            private readonly IComparer<TResult> _comparer;

            public ICDF_sink(IObserver<随机变量值域> observer, IDisposable cancel, IDictionary<随机变量值域, TResult> dict, IComparer<TResult> comparer)
                :base(observer,cancel)
            {
                _dictionary = dict;
                _comparer = comparer;
            }

            public void OnNext(TResult value)
            {

                var keyValue = getICDFvalue(value);
                base._observer.OnNext(keyValue);

            }

            // The core code of ICDF 
            // In this part we can get certain 随机变量值域 value according to the probility sequence.
            private 随机变量值域 getICDFvalue(TResult value)
            {
               // There we apply some algorithm to implement the search function.
                var returnvalue = default(随机变量值域);
                var probility=default(TResult);
                foreach(var item in _dictionary.Keys)
                {
                    _dictionary.TryGetValue(item,out probility);
                    if (_comparer.Compare(value, probility) <= 0)
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

