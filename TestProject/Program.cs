using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var mySeries = new SortedList<DateTime, double>();
            mySeries.Add(new DateTime(2011, 01, 1), 10);
            mySeries.Add(new DateTime(2011, 01, 2), 25);
            mySeries.Add(new DateTime(2011, 01, 3), 30);
            mySeries.Add(new DateTime(2011, 01, 4), 45);
            mySeries.Add(new DateTime(2011, 01, 5), 50);
            mySeries.Add(new DateTime(2011, 01, 6), 65);

            var calcs = new calculations();
            var avg = calcs.MovingAverage(mySeries, 3);
            foreach (var item in avg)
            {
                Console.WriteLine("{0} {1}", item.Key, item.Value);
            }


 // I find these codes on the web to see how Rx works.
            // Rx 中  Observable 类对 IObservable 进行了扩展，增加了一些静态，例如代码中的Interval的方法，可以供user使用。
            // Subscribe方法 不在 Rx 的命名空间之下，但是也是采用同样的方法实现了对 IObservable 的方法扩展。

            IObservable<long> source =Observable.Interval(TimeSpan.FromSeconds(1));

            IDisposable subscription1 = source.Subscribe(
                            x => Console.WriteLine("Observer 1: OnNext: {0}", x),
                            ex => Console.WriteLine("Observer 1: OnError: {0}", ex.Message),
                            () => Console.WriteLine("Observer 1: OnCompleted"));

            IDisposable subscription2 = source.Subscribe(
                            x => Console.WriteLine("Observer 2: OnNext: {0}", x),
                            ex => Console.WriteLine("Observer 2: OnError: {0}", ex.Message),
                            () => Console.WriteLine("Observer 2: OnCompleted"));

            Console.WriteLine("Press any key to unsubscribe");
            Console.ReadLine();
            subscription1.Dispose();
            subscription2.Dispose();


            Console.ReadLine();

        }
    }


    class calculations
    {
        public SortedList<DateTime, double> MovingAverage(SortedList<DateTime, double> series, int period)
        {
            var result = new SortedList<DateTime, double>();

            for (int i = 0; i < series.Count(); i++)
            {
                if (i >= period - 1)
                {
                    double total = 0;
                    for (int x = i; x > (i - period); x--)
                        total += series.Values[x];
                    double average = total / period;
                    result.Add(series.Keys[i], average);
                }

            }
            return result;
        }
    }


}
