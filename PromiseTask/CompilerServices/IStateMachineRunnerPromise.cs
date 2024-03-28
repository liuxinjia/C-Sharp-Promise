using System;
using LightAsync;

namespace LightAsync.CompilerServices
{
    internal interface IStateMachineRunnerPromise
    {
        Action MoveNext { get; }
        void Return();
    }

    internal interface IStateMachineRunnerPromise<T>
    {
        Action MoveNext { get; }
        PromiseTask<T> Task { get; }
        void SetResult(T result);
        void SetException(Exception exception);
    }

}