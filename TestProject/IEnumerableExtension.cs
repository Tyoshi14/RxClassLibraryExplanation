using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public static class IEnumerableExtension
    {
        // Note that extension methods must define in another static class. 
        public static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator){

                if (source == null) throw new ArgumentNullException("source");
                if (seed == null) throw new ArgumentNullException("seed");
                if (accumulator == null) throw new ArgumentNullException("accumulator");

                // 另外，在程序代码过中，使用using，可以在using结束时，回收所有using段内的内存。
                using (var i = source.GetEnumerator())
                {
                    if (!i.MoveNext())
                    {
                        throw new InvalidOperationException("Sequence contains no elements");
                    }
                    var acc = accumulator(seed, i.Current);

                    while (i.MoveNext())
                    {
                        yield return acc;
                        acc = accumulator(acc, i.Current);
                    }
                    yield return acc;
                }
            }


        public static IEnumerable<double> MovingAverage<T>(this IEnumerable<T> inputStream, Func<T, double> selector, int period)
        {
            var ma = new MovingAverage(period);
            foreach (var item in inputStream)
            {
                ma.Push(selector(item));
                // Indicate that the return is used as iterator.
                yield return ma.Current;
            }
        }

        public static IEnumerable<double> MovingAverage(this IEnumerable<double> inputStream, int period)
        {
            var ma = new MovingAverage(period);
            foreach (var item in inputStream)
            {
                ma.Push(item);
                yield return ma.Current;
            }
        }

    
    

    }

}
