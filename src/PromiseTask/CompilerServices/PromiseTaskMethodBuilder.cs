using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cr7Sund;

namespace Cr7Sund.CompilerServices
{
    public struct PromiseTaskMethodBuilder<T>
    {
        private IStateMachineRunnerPromise<T> runnerPromise;
        private T result;
        private Exception ex;

        public PromiseTask<T> Task
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (runnerPromise == null)
                {
                    if (ex != null)
                    {
                        return PromiseTask<T>.FromException(ex);
                    }
                    return PromiseTask<T>.FromResult(result);
                }
                else
                {
                    return runnerPromise.Task;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static PromiseTaskMethodBuilder<T> Create()
            => new PromiseTaskMethodBuilder<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T result)
        {
            if (runnerPromise != null)
            {
                runnerPromise.SetResult(result);
            }
            else
            {
                this.result = result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            if (runnerPromise != null)
            {
                runnerPromise.SetException(exception);
            }
            else
            {
                ex = exception;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AsyncMethodBuilderCore<TStateMachine, T>.SetStateMachine(ref stateMachine, ref runnerPromise);
            awaiter.OnCompleted(runnerPromise.MoveNext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        ///  register itself as the task’s continuation.
        /// </summary>
        /// <typeparam name="TAwaiter"></typeparam>
        /// <typeparam name="TStateMachine"></typeparam>
        /// <param name="awaiter"></param>
        /// <param name="stateMachine"></param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            // aovid boxed operation (interface to struct)
            AsyncMethodBuilderCore<TStateMachine, T>.SetStateMachine(ref stateMachine, ref runnerPromise);
            awaiter.UnsafeOnCompleted(runnerPromise.MoveNext);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // don't use boxed stateMachine.
        }
    }

}