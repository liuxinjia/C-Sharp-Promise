using BenchmarkDotNet.Attributes;
using Cr7Sund.Promises;


namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    public class PromiseGenericBenchMarkTest
    {

        [Benchmark]
        public void CreatePromise()
        {
            var promise = new Promise<int>();
        }

        [Benchmark]
        public void ResolvePromise()
        {
            var promise = new Promise<int>();
            promise.Resolve(2);
        }


        [Benchmark]
        public void RejectPromise()
        {
            var promise = new Promise<int>();
            promise.Reject(new Exception());
        }

        [Benchmark]
        public void ProgressPromise()
        {
            var promise = new Promise<int>();
            promise.ReportProgress(1f);
        }

        [Benchmark]
        public void ResolveActionPromise()
        {
            var promise = new Promise<int>();
            promise.Then((v) => { });
            promise.Resolve(2);
        }

        [Benchmark]
        public async Task ResolvePromiseWithTask()
        {
            var promise = new Promise<int>();
            await Task.Delay(1);
            promise.Resolve(2);
        }

        [Benchmark]
        public void ChainPromisesConvert()
        {
            var promise = new Promise<int>();
            var result = 0;
            var chainedPromise = new Promise<int>();

            promise.Then<int>(v =>
            {
                v += 1;
                return chainedPromise;
            }).Then((v) =>
            {
                result += v;
            });

            chainedPromise.Resolve(0);
            promise.Resolve(result);
        }

        [Benchmark]
        public void ChainPromises()
        {
            var promise = new Promise<int>();
            var result = 0;

            promise
                .Then((v) => v + 1)
                .Then(v =>
                {
                    result = v + 1;
                });

            promise.Resolve(result);
        }
    }
}
