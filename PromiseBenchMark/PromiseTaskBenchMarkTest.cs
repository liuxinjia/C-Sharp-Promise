using BenchmarkDotNet.Attributes;
using RSG.Promises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    public class PromiseTaskBenchMarkTest
    {
        [Benchmark]
        public async Task ResolveAsyncPromise()
        {
            var promise = Promise<int>.Create();
            await promise.ResolveAsync(2);
        }

        [Benchmark]
        public async Task ResolvePromiseWithTask()
        {
            var promise = new Promise<int>();
            await Task.Delay(1);
            await promise.ResolveAsync(2);
        }

        [Benchmark]
        public async Task ChainPromisesAsync()
        {
            var promise = Promise<int>.Create();
            var chainedPromise = Promise<int>.Create();
            var result = 0;

            result = await promise.ResolveAsync(result);
            result = await chainedPromise.ResolveAsync(result);
        }

    }
}
