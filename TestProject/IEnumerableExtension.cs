using System;
using System.Collections.Generic;

namespace TestProject
{
    public static class IEnumerableExtension
    {
        // Note that extension methods must define in another static class. 
        public static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate zero, Func<TAccumulate, TSource, TAccumulate> plus)
        {

            if(source == null)
                throw new ArgumentNullException("source");
            if(zero == null)
                throw new ArgumentNullException("seed");
            if(plus == null)
                throw new ArgumentNullException("accumulator");
            // 另外，在程序代码过中，使用using，可以在using结束时，回收所有using段内的内存。
            // It's a syntactic sugar for IDisposable.Dispose().
            using (var i = source.GetEnumerator())
            {
                // note that users must call IEnumerator.MoveNext() before use the first IEnumerator.Current
                if(!i.MoveNext())
                    throw new InvalidOperationException("Sequence contains no elements");
                var sum = plus(zero, i.Current);
                while(i.MoveNext())
                {
                    yield return sum;
                    sum = plus(sum, i.Current);
                }
                yield return sum;
            }
        }
        public static IEnumerable<double> MovingAverage<T>(this IEnumerable<T> inputStream, Func<T, double> selector, int period)
        {
            var ma = new MovingAverage(period);
            foreach(var item in inputStream)
            {
                ma.Push(selector(item));
                // Indicate that the return is used as iterator.
                // It's a syntactic sugar for IEnumerable<double>.GetEnumerator().
                yield return ma.Current;
            }
        }
        public static IEnumerable<double> MovingAverage(this IEnumerable<double> inputStream, int period)
        {
            var ma = new MovingAverage(period);
            foreach(var item in inputStream)
            {
                ma.Push(item);
                yield return ma.Current;
            }
        }
    }
}
