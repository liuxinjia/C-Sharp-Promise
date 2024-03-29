using BenchmarkDotNet.Attributes;
using RSG.Promises;

namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    [Config(typeof(BenchMarkConfig))]
    public class PromiseTaskBenchMarkTest
    {
        [Benchmark]
        public async Task ResolveAsyncPromise()
        {
            var promise = Promise.Create();
            await promise.ResolveAsync();
        }

        [Benchmark]
        public async Task ResolvePromiseWithTask()
        {
            var promise = new Promise();
            await Task.Delay(1);
            await promise.ResolveAsync();
        }

        [Benchmark]
        public async Task ChainPromisesAsync()
        {
            var promise = Promise.Create();
            var chainedPromise = Promise.Create();

            await promise.ResolveAsync();
            await chainedPromise.ResolveAsync();
        }

    }
}
