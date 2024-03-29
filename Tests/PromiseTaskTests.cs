using RSG.Promises;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RSG.Promises.Tests
{
    public class PromiseTaskTests
    {
        [Fact]
        public async Task<int> ResolvePromise()
        {
            // recommend use
            //return await 2.ToPromiseTask();

            async PromiseTask<int> ResolveInternal()
            {
                var testPromise = Promise<int>.Create();
                testPromise.Resolve(2);
                return await testPromise.AsTask();
            }

            var result = await ResolveInternal();
            Assert.Equal(2, result);

            return result;
        }

        [Fact]
        public async Task<int> ResolveValue()
        {
            async PromiseTask<int> ResolveInternal()
            {
                return await PromiseTask<int>.FromResult(2);
            }

            var result = await ResolveInternal();
            Assert.Equal(2, result);

            return result;
        }

        [Fact]
        public async Task<int> RejectPromise()
        {
            // recommend use
            //return await new Exception().ToPromiseTask<int>();
            const string Message = "HelloWorld";

            async PromiseTask<int> ResolveInternal()
            {
                var promise = Promise<int>.Create();
                promise.Reject(new System.NotImplementedException(Message));
                return await promise.AsTask();
            }

            string resultEx = null;
            try
            {
                await ResolveInternal();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    resultEx = ex.InnerException.Message;
                }
                else
                {
                    resultEx = ex.Message;
                }
            }

            Assert.Equal(Message, resultEx);
            return 2;
        }

        [Fact]
        public async Task<int> RejectedValue()
        {
            const string Message = "HelloWorld";

            async PromiseTask<int> ResolveInternal()
            {
                return await PromiseTask<int>.FromException(new Exception(Message));
            }

            string resultEx = null;
            try
            {
                await ResolveInternal();
            }
            catch (Exception ex)
            {
                resultEx = ex.Message;
            }

            Assert.Equal(Message, resultEx);
            return 2;
        }

        [Fact]
        public async Task ChainPromise()
        {
            var promise = Promise<int>.Create();
            var completed = 0;
            var result = 0;

            promise.Then<int>((v) => result += 1);
            await promise.ResolveAsync(result);
            completed++;

            Assert.Equal(1, completed);
            Assert.Equal(1, result);
        }

#if DEBUG
        [Fact]
        public void PoolAction()
        {
            // setup
            var firstPromise = new Promise<int>();
            firstPromise.Then(_ => { });
            firstPromise.Resolve(2);

            int size = Promise<int>.Test_GetResolveListPoolCount();
            var promise = new Promise<int>();
            promise.Then(_ => { });
            Assert.Equal(size - 1, Promise<int>.Test_GetResolveListPoolCount());

            promise.Resolve(2);
            Assert.Equal(size, Promise<int>.Test_GetResolveListPoolCount());
        }

        [Fact]
        public async Task PoolTaskPromise()
        {
            //setup
            var firstPromise = Promise<int>.Create();
            firstPromise.Then(_ => { });
            await firstPromise.ResolveAsync(2);

            int promiseSize = Promise<int>.Test_GetPoolCount();
            int actionSize = Promise<int>.Test_GetResolveListPoolCount();

            var promise = Promise<int>.Create();
            promise.Then(_ => { });
            Assert.Equal(promiseSize - 1, Promise<int>.Test_GetPoolCount());
            Assert.Equal(actionSize - 1, Promise<int>.Test_GetResolveListPoolCount());
            await promise.ResolveAsync(2);
            Assert.Equal(promiseSize, Promise<int>.Test_GetPoolCount());
            Assert.Equal(actionSize, Promise<int>.Test_GetResolveListPoolCount());
        }

#endif
        [Fact]
        public async Task sequentialStart()
        {
            int result = 2;

            result = await resolveAfter2Seconds(result);
            result = await resolveAfter1Second(result);

            Assert.Equal(result, 21);
        }

        [Fact]
        public async Task sequentialWait()
        {
            int result = 2;


            var slow = resolveAfter2Seconds(result);
            var fast = resolveAfter1Second(result);

            var result1 = await slow;
            var result2 = await fast;

            Assert.Equal(result2, 3);
            Assert.Equal(result1, 20);
        }

        [Fact]
        public async Task concurrent1()
        {
            var result = 2;
            var results = await PromiseTask<int>.WhenAll(
                resolveAfter2Seconds(result),
                resolveAfter1Second(result)
              );

            // 2. Log the results together
            Assert.Equal(20, results[0]);
            Assert.Equal(3, results[1]);
        }
        private async PromiseTask<int> resolveAfter2Seconds(int result)
        {
            await Task.Delay(2);

            var promise = Promise<int>.Create();
            result *= 10;
            promise.Resolve(result);
            return await promise.AsTask();
        }

        private async PromiseTask<int> resolveAfter1Second(int result)
        {
            await Task.Delay(1);

            var promise = Promise<int>.Create();
            result++;
            promise.Resolve(result);

            return await promise.AsTask();
        }

    }
}
