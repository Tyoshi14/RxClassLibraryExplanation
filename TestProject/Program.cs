using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
