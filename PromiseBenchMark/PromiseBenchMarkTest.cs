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
            var promise = Promise.Create();
        }

        [Benchmark]
        public void CreatePromise()
        {
            var promise = Promise.Create();
            //promise.Clear();
        }

        //[Benchmark]
        public void ResolvePromise()
        {
            var promise = Promise.Create();
            promise.Resolve();
            promise.Clear();

        }

        //[Benchmark]
        public void RejectPromise()
        {
            var promise = Promise.Create();
            promise.Reject(new Exception());
        }
        //[Benchmark]
        public void ProgressPromise()
        {
            var promise = Promise.Create();
            promise.ReportProgress(1f);
        }
        //[Benchmark]
        public void ChainPromise()
        {
            var promise = Promise.Create();
            var chainedPromise = Promise.Create();

            var completed = 0;

            promise
                .Then(() => chainedPromise)
                .Then(() => ++completed);

            promise.Resolve();
            chainedPromise.Resolve();
        }
    }
}
