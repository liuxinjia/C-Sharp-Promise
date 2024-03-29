using System;
using RSG;

namespace RSG.CompilerServices
{
    internal interface IStateMachineRunnerPromise
    {
        Action MoveNext { get; }
        PromiseTask Task { get; }

        void SetResult();
        void SetException(Exception exception);
    }

    internal interface IStateMachineRunnerPromise<T>
    {
        Action MoveNext { get; }
        PromiseTask<T> Task { get; }
        void SetResult(T result);
        void SetException(Exception exception);
    }

}