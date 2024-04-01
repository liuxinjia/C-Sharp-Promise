using BenchmarkDotNet.Attributes;
using RSG;
using RSG.Promises;


namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    public class RSGPromiseBenchMarkTest
    {
        void DoSomething()
        {

        }
        private Action doSomething;
        [GlobalSetup]
        public void Setup()
        {
            doSomething = DoSomething;
        }

        [Benchmark]
        public void CreatePromise()
        {
            var promise = new Promise();
        }

        [Benchmark]
        public void ResolvePromise()
        {
            var promise = new Promise();
            promise.Resolve();
        }

        [Benchmark]
        public void RejectPromise()
        {
            var promise = new Promise();
            promise.Reject(new Exception());
        }

        [Benchmark]
        public void ProgressPromise()
        {
            var promise = new Promise();
            promise.ReportProgress(1f);
        }

        [Benchmark]
        public void ResolveActionPromise()
        {
            var promise = new Promise();
            promise.Then(doSomething);
            promise.Resolve();
        }

        [Benchmark]
        public async Task ResolvePromiseWithTask()
        {
            var promise = new Promise();
            promise.Then(doSomething);

            async Task ResolveInternal(Promise p1)
            {
                await Task.Delay(1);
                p1.Resolve();
            }
            await ResolveInternal(promise);
        }


        [Benchmark]
        public void Convert_to_non_value_promise()
        {
            var promise = new Promise<int>();
            var chainedPromise = new Promise();

            const int promisedValue = 15;

            promise
                .Then(v => (IPromise)chainedPromise);

            promise.Resolve(promisedValue);
            chainedPromise.Resolve();
        }

        [Benchmark]
        public void Convert_to_value_promise()
        {
            var chainedPromise = new Promise<int>();
            var promise = new Promise();

            const int promisedValue = 15;

            promise
                .Then(() => (IPromise)chainedPromise);

            promise.Resolve();
            chainedPromise.Resolve(promisedValue);
        }

        [Benchmark]
        public void ChainPromises()
        {
            var promise = new Promise();
            var chainedPromise = new Promise();

            promise
                .Then(() => chainedPromise)
                .Then(() => { });

            promise.Resolve();
            chainedPromise.Resolve();
        }


    }
}
