﻿using System;
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
            subscription1.Dispose();
            subscription2.Dispose();


       // To test observable.Range
       // testObservableRange();

       // To test Observable.Create()
       //    testObservableCreate();
       
       //  To test observable.Never
       //   testObservableNever();

       //     testObservableRepeat();

      //      testObservableReturn();

       // TestNever
      //     var reslt=Observable.Never<int>().Subscribe(new myob());

       // Now I can debug this code[  Observable.Range(1, 2).Aggregate((x, y) => x = x + y)], but still I can't understand the inner logic of Observable.subScribe().
       // For the reason that I see no calls for subscribe or run.Is there some problems with my debug settings??
       // There is no problem with my debug setting. The reason is that I dont call subscribe() really!!
       // Observable.Range(1, 2).Aggregate((x, y) => x = x + y).FirstOrDefault(). Debug this code will be helpful for understanding!!!

            Observable.Range(1, 2).Aggregate((x, y) => x = x + y).FirstOrDefault();
            Console.ReadLine();
        }
        private class myob : IObserver<int>
        {

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnNext(int value)
            {
                throw new NotImplementedException();
            }
        }
        private static void testObservableReturn()
        {
            Console.WriteLine(Observable.Return<int>(100).FirstOrDefault());
        }

        private static void testObservableRepeat()
        {
            Console.WriteLine(Observable.Repeat<int>(5, 10).Count().FirstOrDefault());
        }

        private static void testObservableNever()
        {
            ///  Observable.Never() will never terminate. One writing style
            double witness = 0.0;
            Console.WriteLine(Observable.Never(witness).ToString());
            // another writing style 
            Console.WriteLine(Observable.Never<double>().ToString());
        }


        public  static void testObservableRange() {
           
            //var sumOfNumbers = Observable.Range(1, 10)
            //       .Aggregate(2, (x, y) => x + y, (x) => x - 30).FirstOrDefault();

            //var sumOfNumbers2 = Observable.Range(1, 20)
            //      .Aggregate(0, (x, y) => x + y, (x) => x + 55).FirstOrDefault();

            //Console.WriteLine("Sum of numbers  are  {0} and {1}", sumOfNumbers, sumOfNumbers2);
            var sumOfNumbers = Observable.Range(1, 10);

            for (int i = 0; i < sumOfNumbers.Count<int>().FirstOrDefault() ; i++)
            {
                Console.WriteLine( sumOfNumbers.ElementAt(i).FirstOrDefault()+"\n");
            }
          
            Console.ReadLine();


        }

        public static void testObservableCreate()
        {
            var ob = Observable.Create<string>(
             observer =>
             {
                 var timer = new System.Timers.Timer();
                 timer.Interval = 1000;
                 timer.Elapsed += (s, e) => observer.OnNext("tick");
                 timer.Elapsed += (s, e) => Console.WriteLine(e.SignalTime);
                 timer.Start();
                 return timer;
             });

            var subscription = ob.Subscribe(Console.WriteLine);
            Console.ReadLine();
            subscription.Dispose();
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
