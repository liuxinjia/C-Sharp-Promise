using BenchmarkDotNet.Attributes;
using Cr7Sund;
using Cr7Sund.Promises;

namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    public class PromiseTaskGenericBenchMarkTest
    {
        [Benchmark]
        public async Task ResolveAction()
        {
            async PromiseTask ResolveInternal(int value)
            {
                var promise = Promise<int>.Create();
                // value++; or value.AsValueTask();
                await promise.ResolveAsync(value);
            }

            await ResolveInternal(2);
        }

        [Benchmark]
        public async Task ResolvePromiseWithTask()
        {
            var promise = Promise<int>.Create();
            await Task.Delay(1);
            await promise.ResolveAsync(2);
        }

        [Benchmark]
        public async Task ChainPromises()
        {
            var result = 0;

            async PromiseTask<int> ResolveInternal(int result)
            {
                var promise = Promise<int>.Create();
                result++;
                return await promise.ResolveAsync(result);
            }

            async PromiseTask<int> ResolveChildInternal(int result)
            {
                var promise = Promise<int>.Create();
                result++;
                return await promise.ResolveAsync(result);
            }

            result = await ResolveInternal(result);
            result = await ResolveChildInternal(result);
        }

    }
}
