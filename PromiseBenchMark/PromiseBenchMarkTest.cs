using BenchmarkDotNet.Attributes;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromiseBenchMark
{
    [MemoryDiagnoser]
    public class PromiseBenchMarkTest
    {
        [GlobalSetup]
        public void SetUp()
        {
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
            promise.Clear();

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
        public void ChainPromise()
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

        [Benchmark]
        public void can_chain_promise_and_convert_to_non_value_promise()
        {
            var promise = new Promise<int>();
            var chainedPromise = new Promise();

            const int promisedValue = 15;

            promise
                .Then(v => (IPromise)chainedPromise);

            promise.Resolve(promisedValue);
            chainedPromise.Resolve();
        }
    }
}
