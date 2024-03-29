using System;
using System.Threading.Tasks;
using Xunit;

namespace RSG.Promises.Tests
{
    public class PromiseTaskNoGenericTests
    {
        private int result = 0;

        [Fact]
        public async void ResolvePromise()
        {
            result = 0;
            async PromiseTask ResolveInternal()
            {
                var testPromise = Promise.Create();
                testPromise.Then(() => result++);
                testPromise.Resolve();
                await testPromise.AsTask();
            }

            await ResolveInternal();

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task RejectPromise()
        {
            // recommend use
            // await new Exception().ToPromiseTask();
            const string Message = "HelloWorld";

            async PromiseTask ResolveInternal()
            {
                var promise = Promise.Create();
                promise.Reject(new System.NotImplementedException(Message));
                await promise.AsTask();
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
        }

        [Fact]
        public async Task RejectedValue()
        {
            const string Message = "HelloWorld";

            async PromiseTask ResolveInternal()
            {
                await PromiseTask.FromException(new Exception(Message));
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
        }

        [Fact]
        public async Task ResolvePromiseRepeatedly()
        {
            var promise = Promise.Create();
            string resultEx = null;

            async PromiseTask ResolveInternal()
            {
                await promise.ResolveAsync();
            }

            async PromiseTask RepeatResolveInternal()
            {
                await promise.ResolveAsync();
            }

            await ResolveInternal();
            try
            {
                await RepeatResolveInternal();
            }
            catch (Exception ex)
            {
                resultEx = ex.Message;
            }

            Assert.Equal("item is alread recycled", resultEx);
        }

       
#if DEBUG
        [Fact]
        public void PoolAction()
        {
            // setup
            var firstPromise = new Promise();
            firstPromise.Then(() => { });
            firstPromise.Resolve();

            int size = Promise.Test_GetResolveListPoolCount();
            var promise = new Promise();
            promise.Then(() => { });
            Assert.Equal(size - 1, Promise.Test_GetResolveListPoolCount());

            promise.Resolve();
            Assert.Equal(size, Promise.Test_GetResolveListPoolCount());
        }

        [Fact]
        public async Task PoolTaskPromise()
        {
            //setup
            var firstPromise = Promise.Create();
            firstPromise.Then(() => { });
            await firstPromise.ResolveAsync();

            int promiseSize = Promise.Test_GetPoolCount();
            int actionSize = Promise.Test_GetResolveListPoolCount();

            var promise = Promise.Create();
            promise.Then(() => { });
            Assert.Equal(promiseSize - 1, Promise.Test_GetPoolCount());
            Assert.Equal(actionSize - 1, Promise.Test_GetResolveListPoolCount());
            await promise.ResolveAsync();
            Assert.Equal(promiseSize, Promise.Test_GetPoolCount());
            Assert.Equal(actionSize, Promise.Test_GetResolveListPoolCount());
        }

#endif
        [Fact]
        public async Task sequentialStart()
        {
            result = 2;

            await resolveAfter2Seconds();
            Assert.Equal(result, 20);
            await resolveAfter1Second();

            Assert.Equal(result, 21);
        }

        [Fact]
        public async Task sequentialWait()
        {
             result = 2;

            var slow = resolveAfter2Seconds();
            var fast = resolveAfter1Second();

            await slow;
            Assert.Equal(result, 30);
            await fast;
            Assert.Equal(result, 30);
        }

        [Fact]
        public async Task concurrent1()
        {
            result = 2;
            await PromiseTask.WhenAll(
               resolveAfter2Seconds(),
               resolveAfter1Second()
             );

            Assert.Equal(30, result);
        }

        private async PromiseTask resolveAfter2Seconds()
        {
            await Task.Delay(2);

            var promise = Promise.Create();
            result *= 10;
            promise.Resolve();
            await promise.AsTask();
        }

        private async PromiseTask resolveAfter1Second()
        {
            await Task.Delay(1);

            var promise = Promise.Create();
            result++;
            promise.Resolve();

            await promise.AsTask();
        }

    }
}
