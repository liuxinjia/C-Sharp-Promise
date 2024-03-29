using BenchmarkDotNet.Attributes;
using RSG.Promises;


namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    [Config(typeof(BenchMarkConfig))]
    public class PromiseBenchMarkTest
    {

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
            promise.Then(() => { });
            promise.Resolve();
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

            var completed = 0;

            promise
                .Then(() => chainedPromise)
                .Then(() => ++completed);

            promise.Resolve();
            chainedPromise.Resolve();
        }


    }
}
