using Cr7Sund;
using Cr7Sund.Promises;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

//
// Example of downloading text from a URL using a promise.
//
namespace Example
{

    class Program
    {

        static async Task Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            //await sequentialStart();
            //await sequentialWait();
            //await concurrent1();
            //await concurrent2();


            await ResolvePromiseWithTask();

            stopWatch.Stop();
            Console.WriteLine($"Finish : {stopWatch.ElapsedMilliseconds}");
            Console.ReadLine();
        }
        public static async Task ResolvePromiseWithTask()
        {
            var promise = Promise.Create();

            async Task ResolveInternal()
            {
                await Task.Delay(1);
                Console.WriteLine("1");
                promise.Resolve();
                Console.WriteLine("3");
            }
            var task = ResolveInternal();

            Console.WriteLine("0");
            await promise.AsTask();
            Console.WriteLine("2");
            await task;
        }
        private static async PromiseTask<string> resolveAfter2Seconds()
        {
            Console.WriteLine("starting slow promise");

            await Task.Delay(2000);

            var promise = Promise<string>.Create();
            //promise.Resolve("slow");
            Exception ex;
            try
            {
                throw new System.Exception();
            }
            catch (Exception e)
            {
                ex = e;
            }
            promise.Reject(ex);
            promise.Catch((e) =>
            {
                Console.WriteLine(e);
            });
            Console.WriteLine("slow promise is done");

            return await promise.AsTask();
        }

        private static async PromiseTask<string> resolveAfter1Second()
        {
            Console.WriteLine("starting fast promise");

            await Task.Delay(1000);

            var promise = Promise<string>.Create();
            promise.Resolve("fast");
            Console.WriteLine("fast promise is done");

            return await promise.AsTask();
        }


        private static async Task<string> resolveAfter2Seconds_Task()
        {
            const string result = "slow";

            return await Task<string>.Run(async () =>
              {
                  Console.WriteLine("starting slow promise");

                  await Task.Delay(2000);
                  Console.WriteLine(result);
                  await Promise<string>.Create().ResolveAsync(result);
                  Console.WriteLine("slow promise is done");
                  return result;
              });

        }

        private static async Task<string> resolveAfter1Second_Task()
        {
            const string result = "fast";

            return await Task<string>.Run(async () =>
           {
               Console.WriteLine("starting fast promise");

               await Task.Delay(1000);
               Console.WriteLine(result);
               await Promise<string>.Create().ResolveAsync(result);
               Console.WriteLine("fast promise is done");
               return result;
           });

        }

        public static async Task sequentialStart()
        {
            Console.WriteLine("== sequentialStart starts ==");

            // 1. Start a timer, log after it's done
            var slow = resolveAfter2Seconds();
            try
            {
                Console.WriteLine(await slow);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();
            }

            // 2. Start the next timer after waiting for the previous one
            var fast = resolveAfter1Second();
            Console.WriteLine(await fast);

            Console.WriteLine("== sequentialStart done ==");
        }
        public static async Task sequentialWait()
        {
            Console.WriteLine("== sequentialWait starts ==");

            // 1. Start two timers without waiting for each other
            var slow = resolveAfter2Seconds();
            var fast = resolveAfter1Second();

            // 2. Wait for the slow timer to complete, and then log the result
            try
            {
                Console.WriteLine(await slow);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            // 3. Wait for the fast timer to complete, and then log the result
            Console.WriteLine(await fast);

            Console.WriteLine("== sequentialWait done ==");
        }
        public static async Task concurrent1()
        {
            Console.WriteLine("== concurrent1 starts ==");

            try
            {

                // 1. Start two timers concurrently and wait for both to complete
                var results = await PromiseTask<string>.WhenAll(
                    resolveAfter2Seconds(),
                    resolveAfter1Second()
                  );

                // 2. Log the results together
                Console.WriteLine(results[0]);
                Console.WriteLine(results[1]);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
            Console.WriteLine("== concurrent1 done ==");
        }
        public static async Task concurrent2()
        {
            Console.WriteLine("== concurrent2 starts ==");

            // 1. Start two timers concurrently, log immediately after each one is done

            await Task<string>.WhenAll(
                    resolveAfter2Seconds_Task(),
                    resolveAfter1Second_Task()
                 );

            Console.WriteLine("== concurrent2 done ==");
        }
    }
}
