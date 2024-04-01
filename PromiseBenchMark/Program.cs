using BenchmarkDotNet.Running;
using PromiseBenchMark;


var summary = BenchmarkRunner.Run<PromiseTaskBenchMarkTest>();