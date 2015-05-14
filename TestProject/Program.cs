using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;


namespace TestProject
{
    class Program
    {

        static void Main(string[] args)
        {

            //var mySeries = new SortedList<DateTime, double>();
            //mySeries.Add(new DateTime(2011, 01, 1), 10);
            //mySeries.Add(new DateTime(2011, 01, 2), 25);
            //mySeries.Add(new DateTime(2011, 01, 3), 30);
            //mySeries.Add(new DateTime(2011, 01, 4), 45);
            //mySeries.Add(new DateTime(2011, 01, 5), 50);
            //mySeries.Add(new DateTime(2011, 01, 6), 65);

            //var calcs = new calculations();
            //var avg = calcs.MovingAverage(mySeries, 3);
            //foreach (var item in avg)
            //{
            //    Console.WriteLine("{0} {1}", item.Key, item.Value);
            //}



            // I find these codes on the web to see how Rx works.
            // Rx 中  Observable 类对 IObservable 进行了扩展，增加了一些静态，例如代码中的Interval的方法，可以供user使用。
            // Subscribe方法 不在 Rx 的命名空间之下，但是也是采用同样的方法实现了对 IObservable 的方法扩展。

            //IObservable<long> source =Observable.Interval(TimeSpan.FromSeconds(1));

            //IDisposable subscription1 = source.Subscribe(
            //                x => Console.WriteLine("Observer 1: OnNext: {0}", x),
            //                ex => Console.WriteLine("Observer 1: OnError: {0}", ex.Message),
            //                () => Console.WriteLine("Observer 1: OnCompleted"));

            //IDisposable subscription2 = source.Subscribe(
            //                x => Console.WriteLine("Observer 2: OnNext: {0}", x),
            //                ex => Console.WriteLine("Observer 2: OnError: {0}", ex.Message),
            //                () => Console.WriteLine("Observer 2: OnCompleted"));

            //Console.WriteLine("Press any key to unsubscribe");
            //subscription1.Dispose();
            //subscription2.Dispose();


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

            //      Observable.Range(1, 2).Aggregate((x, y) => x = x + y).FirstOrDefault();
            //     Console.ReadLine();


            //     testBufferWithBoundries();


            /// TO TEST IEnumerable MOVEAVERAGE
            // testMoveAverageIEnumerable();

            // Move Average with RX
            //  testMoveAverageWithRx();

            // This is another example that uses Rx to implement the functin of MoveAverage
            // Note that Asympotic time complexity is O(n)
            //testMoveAverageWithRx2();

            // This is a example to finish the moveAverage function with Observable object.
            //moveAverageWithObservable();

            // There begin the function to get the empircial distribution.


            // Use the original red-black tree to coculate the CDF.
            // testECDF();

            // Use the extend data structure to get CDF.

            //var element = tree.getTreeInOrderWalk();
            //foreach (var elem in element)
            //{
            //    Console.Write(elem + " ");
            //}
            //Console.WriteLine();


            //var list = tree.getTreeInLayer();
            //int colum = 0;
            //Console.WriteLine("Root");
            //foreach (var item in list)
            //{
            //    if (item.key == default(int))
            //    {
            //        Console.WriteLine("Column {0}", ++colum);
            //    }
            //    else if (item.isRed)
            //    {
            //        //  subTreeSize is used to see whether we get the right subtree number
            //        Console.WriteLine("   " + item.key + " Red" + " count: " + item.subtreesize);
            //    }
            //    else
            //    {
            //        Console.WriteLine("   " + item.key + " Black" + " count: " + item.subtreesize);
            //    }
            //}

            // To test CDF fand ICDF


            CDFTreeTest1();
            CDFTreeTest2();

            Console.ReadLine();

        }

        private static void CDFTreeTest2()
        {
            TestCDFTree("");
            TestCDFTree("A");
            TestCDFTree("A+B");
            TestCDFTree("B+A");
            TestCDFTree("D[BF][ACEG]");
            TestCDFTree("B[AD][CF][EG]");
            TestCDFTree("F[DG][BE][AC]");
            TestCDFTree("B[AF][DG][CE]");
            TestCDFTree("F[BG][AD][CE]");
            Random r = new Random();
            for(int i = 0; i < 10; i++)
            {
                int n = r.Next(0, 512);
                char[] cs = new char[n];
                for(int j = 0; j < n; j++)
                    cs[j] = (char)('A' + r.Next(26));
                TestCDFTree(new string(cs));
            }

        }

        private static void TestCDFTree(string testInput)
        {
            Console.WriteLine("Testing: {0}", testInput);
            string expectedOutput = SortedSet<char>.Serielize(SortedSet<char>.Create(testInput), k => k.ToString());
            var tree = CDFTree<char>.Create(testInput);
            string testOutput = CDFTree<char>.Serielize(tree, k => k.ToString());
            Console.WriteLine("Expecting: {0}", expectedOutput);
            Console.WriteLine("Get:       {0}", testOutput);
            if(testOutput.Equals(expectedOutput))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Pass!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fail!");
            }
            Console.ResetColor();
            var freqDict = new System.Collections.Generic.Dictionary<char, int>();
            int n = 0;
            foreach(var k in testInput)
                if(k >= 'A' && k <= 'Z')
                {
                    n++;
                    if(freqDict.ContainsKey(k))
                        freqDict[k]++;
                    else
                        freqDict[k] = 1;
                }
            int m= 0;
            for(char k = 'A'; k <= 'Z'; k++)
            {
                m += freqDict.ContainsKey(k) ? freqDict[k] : 0;
                if((double)m/n==tree.CDF(k))
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(k);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void CDFTreeTest1()
        {
            CDFTree<int> tree = new CDFTree<int>();
            int[] array ={
1   ,
50  ,
2   ,
37  ,
5   ,
26  ,
10  ,
17  ,
26  ,
2
 };
            for(int i = 0; i < array.Length; i++)
                tree.Add(array[i], 1);
            for(int j = 0; j <= 51; j++)
            {
                double value = tree.CDF(j);
                int expect;
                if(j < 1)
                {
                    expect = 1;//this is wrong
                    if(value != 0)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(j < 2)
                {
                    expect = 1;
                    if(value != 0.1)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(j < 5)
                {
                    expect = 2;
                    if(value != 0.3)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(j < 10)
                {
                    expect = 5;
                    if(value != 0.4)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(j < 17)
                {
                    expect = 10;
                    if(value != 0.5)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(j < 26)
                {
                    expect = 17;
                    if(value != 0.6)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(j < 37)
                {
                    expect = 26;
                    if(value != 0.8)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else if(j < 50)
                {
                    expect = 37;
                    if(value != 0.9)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    expect = 50;
                    if(value != 1)
                        Console.ForegroundColor = ConsoleColor.Red;
                }
                var icdfp = tree.ICDF(value);
                if(expect != icdfp)
                    Console.ForegroundColor = ConsoleColor.Magenta;
                //Console.WriteLine(j);
                Console.WriteLine(" " + j + " 概率 " + value + " 的结果为 " + icdfp);
                //Console.WriteLine();
                Console.ResetColor();
            }
            Console.WriteLine("CDFTreeTest1 Finished.");
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


        private static void moveAverageWithObservable()
        {
            var delta = 3;
            var series = new[] { 1.1, 2.5, 3.8, 4.8, 5.9, 6.1, 7.6 };
            var newSeries = series.ToObservable();

            int count = 0;

            var seed = default(Double);
            newSeries.Take(delta).Sum().Subscribe(x => seed = x);
            // There I can get the  correct sum.
            //  Console.WriteLine(seed);
            var result = Observable.Repeat(0.0, delta - 1).Concat(Observable.Repeat(seed / delta, 1));


            //newSeries.Skip(delta)
            //    // .Catch(newSeries)
            //   // .Merge(newSeries)
            //   // .Aggregate(seed, (x, y) => seed = (seed - x + y) / delta)
            //    .Subscribe(Console.WriteLine);



            //newSeries.Window(delta).ForEach(ob => {

            //    Console.Write(count+++"\t");
            //    ob.Average().Subscribe(Console.WriteLine);
            //});

            var avarega = newSeries.Zip<double, double, double>(newSeries.Skip(delta), (x, y) => {
                // Console.Write(count++ + ":\t " + x.ToString() + " " + y.ToString() + "\t");
                seed = seed - x + y;
                return seed / delta;
            });

            result.Concat(avarega).Subscribe(ob => {
                Console.WriteLine(count++ + ":\t " + ob);
            });


        }


        private static void testMoveAverageWithRx2()
        {
            var mySeries = new SortedList<DateTime, double>();
            mySeries.Add(new DateTime(2011, 01, 1), 10);
            mySeries.Add(new DateTime(2011, 01, 2), 25);
            mySeries.Add(new DateTime(2011, 01, 3), 30);
            mySeries.Add(new DateTime(2011, 01, 4), 45);
            mySeries.Add(new DateTime(2011, 01, 5), 50);
            mySeries.Add(new DateTime(2011, 01, 6), 65);

            int period = 3;

            var results = mySeries.Skip(period - 1).Aggregate(
                new {
                    Result = new SortedList<DateTime, double>(),
                    Working = new List<double>(mySeries.Take(period - 1).Select(item => item.Value))
                },
                (list, item) => {
                    list.Working.Add(item.Value);
                    list.Result.Add(item.Key, list.Working.Average());
                    list.Working.RemoveAt(0);
                    return list;
                }
              ).Result;

            int count = 0;
            foreach(var item in results)
            {
                count++;
                Console.WriteLine(count + " : " + item + "\n");
            }
        }

        private static void testMoveAverageWithRx()
        {
            var delta = 3;
            var series = new[] { 1.1, 2.5, 3.8, 4.8, 5.9, 6.1, 7.6 };

            // Take: Returns a specified number of contiguous elements from the start of a sequence.
            // Skip:  Bypasses a specified number of elements in a sequence and then returns the
            //     remaining elements.
            // Zip : Merges two sequences by using the specified predicate function.
            var seed = series.Take(delta).Average();
            //with a minor modification smas is an IObservable now
            var smas = series.ToObservable()
                .Skip(delta)
                .Zip(series, Tuple.Create)
                .Scan(seed, (sma, values) => sma - (values.Item2 / delta) + (values.Item1 / delta));

            // Repeat :  Generates a sequence that contains one repeated value.
            // Concat: Concatenates two sequences.

            smas = Observable.Repeat(double.NaN, delta - 1).Concat(new[] { seed }.ToObservable()).Concat(smas);
            // There we use the zip Function to print out the results.
            var _ = smas.Zip(Observable.Range(0, series.Length), (x, i) => {
                Console.WriteLine(i + " : " + x + "\n");
                return x;
            }).Wait();
        }


        private static void testMoveAverageIEnumerable()
        {
            double[] intergers = new double[100];
            for(int i = 0; i < 100; i++)
            {
                intergers[i] = (double)i;
            }

            IEnumerable<double> result = intergers.MovingAverage(10);
            int count = 0;
            foreach(var item in result)
            {
                count++;
                Console.WriteLine(count + " : " + item + "\n");
            }
        }


        private static void testBufferWithBoundries()
        {
            int countNum = 0;
            var newsource = Observable.Range(1, 10);
            newsource.Buffer(newsource.Scan((a, c) => a + c).SkipWhile(a => a < 0)).Subscribe(
                            x => Console.WriteLine("Number {0} : OnNext: {1}", countNum++, x.FirstOrDefault()),
                            ex => Console.WriteLine("Number {0} : OnError: {1}", countNum - 1, ex.Message),
                            () => Console.WriteLine("Number {0}: OnCompleted", countNum - 1)
                );
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


        public static void testObservableRange()
        {

            //var sumOfNumbers = Observable.Range(1, 10)
            //       .Aggregate(2, (x, y) => x + y, (x) => x - 30).FirstOrDefault();

            //var sumOfNumbers2 = Observable.Range(1, 20)
            //      .Aggregate(0, (x, y) => x + y, (x) => x + 55).FirstOrDefault();

            //Console.WriteLine("Sum of numbers  are  {0} and {1}", sumOfNumbers, sumOfNumbers2);
            var sumOfNumbers = Observable.Range(1, 10);

            for(int i = 0; i < sumOfNumbers.Count<int>().FirstOrDefault(); i++)
            {
                Console.WriteLine(sumOfNumbers.ElementAt(i).FirstOrDefault() + "\n");
            }

            Console.ReadLine();


        }

        public static void testObservableCreate()
        {
            var ob = Observable.Create<string>(
             observer => {
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

        public static void testECDF()
        {
            // Note that when you test these code, you will find different results.
            // It seems that we use the same variable result and will certainly get the same execition number, but this will never happen because 
            // of the lazy evaluaion of C# and Rx. The variable you use each time is new and generated immediately!
            Random rand = new Random();
            var result = Observable.Range(0, 10).Select((_) => rand.Next(5) + rand.Next(5)).ECDF();
            //result.Subscribe((ecdf) =>
            //{
            //    Console.WriteLine("Start printing dictionary of {0}", ecdf.Count);
            //    foreach (var kv in ecdf)
            //    {
            //        Console.WriteLine("{0},{1}", kv.Key, kv.Value);
            //    }
            //    Console.WriteLine("{0} pairs printed!", ecdf.Count);
            //    Console.WriteLine();
            //});

            var test = Observable.Range(0, 10).CDF<int>(result);
            test.Subscribe(x => Console.WriteLine("IObservable正向查询结果为： " + x));


            //var dict = result.Last();
            //foreach (var kv in dict)
            //{
            //    Console.WriteLine("{0},{1}", kv.Key, kv.Value);
            //}

            //var test = Observable.Repeat(1.0,1).ICDF<int>(result);
            //var test1 = Observable.Repeat(1.0, 1).ICDF<int>(dict);

            //test.Subscribe(x => Console.WriteLine("Observable反向查询结果为： "+x));
            //test1.Subscribe(x => Console.WriteLine("Dictioanry反向查询结果为： " + x));
        }
    }


    class calculations
    {
        public SortedList<DateTime, double> MovingAverage(SortedList<DateTime, double> series, int period)
        {
            var result = new SortedList<DateTime, double>();
            double total = 0;
            for(int i = 0; i < series.Count(); i++)
            {
                //if (i >= period - 1)
                //{
                //    double total = 0;
                //    for (int x = i; x > (i - period); x--)
                //        total += series.Values[x];
                //    double average = total / period;
                //    result.Add(series.Keys[i], average);
                //}

                /// Another way to write this code 
                ///  This way may be faster than the fomer.
                if(i >= period)
                {
                    total -= series.Values[i - period];
                }
                total += series.Values[i];

                if(i >= period - 1)
                {
                    double average = total / period;
                    result.Add(series.Keys[i], average);
                }

            }
            return result;
        }
    }


}
