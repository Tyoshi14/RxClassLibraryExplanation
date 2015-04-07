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
        public static IObservable<TResult> CDF<随机变量值域, TResult>(this IObservable<随机变量值域> searchSource, IObservable<IDictionary<随机变量值域, TResult>> source)
        {
            return new CDF<随机变量值域, TResult>(source, searchSource);
        }

        public static IObservable<TResult> CDF<随机变量值域, TResult>(this IObservable<随机变量值域> searchSource, IDictionary<随机变量值域, TResult> source) {
            return new CDF<随机变量值域, TResult>(source, searchSource);
        }

        public static IObservable<随机变量值域> ICDF<随机变量值域, TResult>(this IObservable<TResult> searchSource, IDictionary<随机变量值域, TResult> dict)
        {
            return new ICDF<随机变量值域, TResult>(searchSource,dict);
   }


   }

    public class CDF<随机变量值域, TResult> : Producer<TResult>
    {
        private readonly IObservable<IDictionary<随机变量值域, TResult>> _source;
        private readonly IObservable<随机变量值域> _searchSource;
        private volatile IDictionary<随机变量值域, TResult> _dict;

        public CDF(IObservable<IDictionary<随机变量值域, TResult>> source, IObservable<随机变量值域> searchSource)
        {
            _source = source;
            _searchSource = searchSource;
            //需要订阅并不断更新最新数据表而不是获得一个静态的。
            source.Subscribe((dict) => _dict = dict);
        }

        public CDF(IDictionary<随机变量值域, TResult> dict, IObservable<随机变量值域> searchSource)
        {
            _dict = dict;
            _searchSource = searchSource;
        }
       
        protected override IDisposable Run(IObserver<TResult> observer, IDisposable cancel, Action<IDisposable> setSink)
        {

            var sink = new CDF_sink(observer, cancel, _dict);
            setSink(sink);
            return _searchSource.SubscribeSafe(sink);
        }



        class CDF_sink : Sink<TResult>, IObserver<随机变量值域>
        {
            private IDictionary<随机变量值域, TResult> dictionary;
            public CDF_sink(IObserver<TResult> observer, IDisposable cancel, IDictionary<随机变量值域, TResult> dict)
                : base(observer, cancel)
            {
                dictionary = dict;
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
                if(dictionary.Keys.Contains(value)){
                    dictionary.TryGetValue(value, out result);
                }else{
                // TO get the value that applies some algorithm methods which need to implement.

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




    public class ICDF<随机变量值域, TResult> : Producer<随机变量值域> {
       
        private readonly IObservable<IDictionary<随机变量值域, TResult>> _source;
        private readonly IObservable<TResult> _searchSource;
        private IDictionary<随机变量值域, TResult> _dict;

        public ICDF(IObservable<TResult> searchSource, IDictionary<随机变量值域, TResult> dict)
        {
            _searchSource = searchSource;
            _dict = dict;
        }

        protected override IDisposable Run(IObserver<随机变量值域> observer, IDisposable cancel, Action<IDisposable> setSink)
        {
            var sink = new ICDF_sink(observer, cancel, _dict);
            setSink(sink);
            return _searchSource.SubscribeSafe(sink);
        }

        class ICDF_sink : Sink<随机变量值域>, IObserver<TResult> 
        {
            private IDictionary<随机变量值域, TResult> _dictionary;
            public ICDF_sink(IObserver<随机变量值域> observer, IDisposable cancel, IDictionary<随机变量值域, TResult> dict)
                :base(observer,cancel)
            {
                _dictionary = dict;
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
                    var comparer = Comparer<TResult>.Default;
;

                if (comparer.Compare(value, probility) <= 0)
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

