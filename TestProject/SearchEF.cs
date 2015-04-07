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
   }

    public class CDF<随机变量值域, TResult> : Producer<TResult>
    {
        private readonly IObservable<IDictionary<随机变量值域, TResult>> _source;
        private readonly IObservable<随机变量值域> _searchSource;
        private IDictionary<随机变量值域, TResult> _dict;

        public CDF(IObservable<IDictionary<随机变量值域, TResult>> source, IObservable<随机变量值域> searchSource)
        {
            _source = source;
            _searchSource = searchSource;
            //需要订阅并不断更新最新数据表而不是获得一个静态的。
            _dict = getDictionary();
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

        // Get the lastest dictioanry we used to get the probability.
        private IDictionary<随机变量值域, TResult> getDictionary()
        {
            if (_source == null) throw new ArgumentNullException("source");
            var dict = default(IDictionary<随机变量值域, TResult>);
            dict = _source.LastOrDefault();
            // Errors!! Empty!! 
            // There the program will never stop!!??? why
            Console.WriteLine(dict.Keys);
            return dict;
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


    }

