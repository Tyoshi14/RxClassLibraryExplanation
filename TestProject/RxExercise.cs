using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;

using System.Data;
using System.Threading;



namespace TestProject
{
    class RxExercise
    {
        //Example 1:  Asynchronous Method
        public static async void StartBackgroundWork()
        {
            Console.WriteLine("Shows use of Start to start on a background thread:");
            var o = Observable.Start(() =>
            {
                //This starts on a background thread.
                Console.WriteLine("From background thread. Does not block main thread.");
                Console.WriteLine("Calculating...");
                Thread.Sleep(1000);
                Console.WriteLine("Background work completed.");
            });
            await o.FirstAsync();   // subscribe and wait for completion of background operation.  If you remove await, the main thread will complete first.
            Console.WriteLine("Main thread completed.");
        }

        // Example 2: Synchronous operation
        public string DoLongRunningOperation(string param)
        {
           
            return "********\n";
           
        }

        public IObservable<string> LongRunningOperationAsync(string param)
        {
            return Observable.Create<string>(
                o => {
                    Console.WriteLine("This is a test!\n");
                    return Observable.ToAsync<string, string>(DoLongRunningOperation)(param).Subscribe(o);
                }
            );
        }

        // Example3  Combine a serious elements.
        public void CombineLatest() {
            var o = Observable.CombineLatest(
            Observable.Start(() => { Console.WriteLine("Executing 1st on Thread: {0}", Thread.CurrentThread.ManagedThreadId); return "Result A"; }),
            Observable.Start(() => { Console.WriteLine("Executing 2nd on Thread: {0}", Thread.CurrentThread.ManagedThreadId); return "Result B"; }),
            Observable.Start(() => { Console.WriteLine("Executing 3rd on Thread: {0}", Thread.CurrentThread.ManagedThreadId); return "Result C"; })
        ).Finally(() => Console.WriteLine("Done!"));

            foreach (string r in o.First())
               Console.WriteLine(r);
        
        }

        // Example4  Create With Disposable & Scheduler - Canceling an asynchronous operation
        public void CancelAsynOperation() {
          
            IObservable<int> ob = Observable.Create<int>(o =>
            {
                
                var cancel = new CancellationDisposable(); // internally creates a new CancellationTokenSource
                NewThreadScheduler.Default.Schedule(() =>
                {
                    int i = 0;
                    for (; ; )
                    {
                        Thread.Sleep(200);  // here we do the long lasting background operation
                        if (!cancel.Token.IsCancellationRequested)    // check cancel token periodically
                            o.OnNext(i++);
                        else
                        {
                            Console.WriteLine("Aborting because cancel event was signaled!");
                            o.OnCompleted();
                            return;
                        }
                    }
                }
                );

                return cancel;
            }
            );

        }


    }
}
