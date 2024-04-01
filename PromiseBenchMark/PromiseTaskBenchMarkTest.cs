using BenchmarkDotNet.Attributes;
using Cr7Sund;
using Cr7Sund.Promises;

namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    [Config(typeof(BenchMarkConfig))]
    public class PromiseTaskBenchMarkTest
    {
        void DoSomething()
        {

        }
        private Action doSomething = null;
        [GlobalSetup]
        public void Setup()
        {
            doSomething = DoSomething;
        }

        //[Benchmark]
        public async Task ResolveAction()
        {
            async PromiseTask ResolveInternal()
            {
                var promise = Promise.Create();
                await promise.ResolveAsync();
                doSomething?.Invoke();
            }
            await ResolveInternal();
        }


        //[Benchmark]
        public async Task ResolvePromiseWithTask()
        {
            var promise = new Promise();
            promise.Then(doSomething);

            async PromiseTask ResolveInternal(Promise p1)
            {
                await Task.Delay(1).ConfigureAwait(true);
                p1.Resolve();
            }

            var task = ResolveInternal(promise);
            await promise.AsTask();
            await task;
        }

        //[Benchmark]
        public async Task ComparisonTask()
        {
            async Task ResolveInternal()
            {
                await Task.Delay(1).ConfigureAwait(true);
            }
            await ResolveInternal();
        }

        [Benchmark]
        public async Task ComparisonTaskAA()
        {
            async Task ResolveInternal()
            {
                await Task.Delay(1).ConfigureAwait(false);
            }
            await ResolveInternal();
        }
        //[Benchmark]
        public async Task ChainPromises()
        {
            async PromiseTask ResolveInternal()
            {
                var promise = Promise.Create();
                await promise.ResolveAsync();
            }

            async PromiseTask ResolveChildInternal()
            {
                var promise = Promise.Create();
                //completed++;
                await promise.ResolveAsync();
            }

            await ResolveInternal();
            await ResolveChildInternal();
        }

    }
}
