using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cr7Sund.CompilerServices
{
#if DEBUG
    internal class ExceptionHolder
    {
        ExceptionDispatchInfo exception;
        bool calledGet = false;

        public ExceptionHolder(ExceptionDispatchInfo exception)
        {
            this.exception = exception;
        }

        public ExceptionDispatchInfo GetException()
        {
            if (!calledGet)
            {
                calledGet = true;
                GC.SuppressFinalize(this);
            }
            return exception;
        }

        ~ExceptionHolder()
        {
            if (!calledGet)
            {
                //UniTaskScheduler.PublishUnobservedTaskException(exception.SourceException);
            }
        }
    }
#endif
    internal static class UniTaskCompletionSourceCoreShared // separated out of generic to avoid unnecessary duplication
    {
        internal static readonly Action s_sentinel = CompletionSentinel;

        private static void CompletionSentinel() // named method to aid debugging
        {
            throw new InvalidOperationException("The sentinel delegate should never be invoked.");
        }
    }

    public struct PromiseTaskCompletionSourceCore
    {
        object error; // ExceptionHolder or OperationCanceledException
        short version;
        bool hasUnhandledError;
        int completedCount; // 0: completed == false
        Action continuation;

        /// <summary>Gets the operation version.</summary>
        public short Version => version;



        public void Reset()
        {
            ReportUnhandledError();

            unchecked
            {
                version += 1; // incr version.
            }
            completedCount = 0;
            error = null;
            hasUnhandledError = false;
            continuation = null;
        }

        void ReportUnhandledError()
        {
            if (hasUnhandledError)
            {
                try
                {
                    if (error is OperationCanceledException oc)
                    {
                        //UniTaskScheduler.PublishUnobservedTaskException(oc);
                    }
                }
                catch
                {
                }
            }
        }

        internal void MarkHandled()
        {
            hasUnhandledError = false;
        }

        /// <summary>Completes with a successful result.</summary>
        /// <param name="result">The result.</param>

        public bool TrySetResult()
        {
            if (Interlocked.Increment(ref completedCount) == 1)
            {
                if (continuation != null || Interlocked.CompareExchange(ref continuation, UniTaskCompletionSourceCoreShared.s_sentinel, null) != null)
                {
                    continuation?.Invoke();
                }
                return true;
            }

            return false;
        }

        /// <summary>Completes with an error.</summary>
        /// <param name="ex">The exception.</param>

        public bool TrySetException(Exception ex)
        {
            if (Interlocked.Increment(ref completedCount) == 1)
            {
                // setup result
                hasUnhandledError = true;
                if (ex is OperationCanceledException)
                {
                    this.error = ex;
                }
                else
                {
#if DEBUG
                    this.error = new ExceptionHolder            (ExceptionDispatchInfo.Capture(ex));
#else
                    this.error = ex;
#endif
                }

                if (continuation != null || Interlocked.CompareExchange(ref continuation, UniTaskCompletionSourceCoreShared.s_sentinel, null) != null)
                {
                    continuation?.Invoke();
                }
                return true;
            }

            return false;
        }

        public bool TrySetCanceled(CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref completedCount) == 1)
            {
                // setup result
                hasUnhandledError = true;
                error = new OperationCanceledException(cancellationToken);

                if (continuation != null || Interlocked.CompareExchange(ref continuation, UniTaskCompletionSourceCoreShared.s_sentinel, null) != null)
                {
                    continuation?.Invoke();
                }
                return true;
            }

            return false;
        }


        /// <summary>Gets the status of the operation.</summary>
        /// <param name="token">Opaque value that was provided to the <see cref="UniTask"/>'s constructor.</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromiseTaskStatus GetStatus(short token)
        {
            ValidateToken(token);
            return continuation == null || completedCount == 0 ? PromiseTaskStatus.Pending
                 : error == null ? PromiseTaskStatus.Succeeded
                 : error is OperationCanceledException ? PromiseTaskStatus.Canceled
                 : PromiseTaskStatus.Faulted;
        }

        /// <summary>Gets the status of the operation without token validation.</summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromiseTaskStatus UnsafeGetStatus()
        {
            return continuation == null || completedCount == 0 ? PromiseTaskStatus.Pending
                 : error == null ? PromiseTaskStatus.Succeeded
                 : error is OperationCanceledException ? PromiseTaskStatus.Canceled
                 : PromiseTaskStatus.Faulted;
        }

        /// <summary>Gets the result of the operation.</summary>
        /// <param name="token">Opaque value that was provided to the <see cref="UniTask"/>'s constructor.</param>
        // [StackTraceHidden]

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult(short token)
        {
            ValidateToken(token);
            if (completedCount == 0)
            {
                throw new InvalidOperationException("Not yet completed, UniTask only allow to use await.");
            }

            if (error != null)
            {
                hasUnhandledError = false;
                if (error is OperationCanceledException oce)
                {
                    throw oce;
                }
#if DEBUG
                else if (error is ExceptionHolder eh)
                {
                    eh.GetException().Throw();
                }
#else
                else if (error is Exception ex)
                {
                    throw ex;
                }
#endif
                throw new InvalidOperationException("Critical: invalid exception type was held.");
            }

        }

        /// <summary>Schedules the continuation action for this operation.</summary>
        /// <param name="continuation">The continuation to invoke when the operation has completed.</param>
        /// <param name="token">Opaque value that was provided to the <see cref="UniTask"/>'s constructor.</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation, short token /*, ValueTaskSourceOnCompletedFlags flags */)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }
            ValidateToken(token);

            /* no use ValueTaskSourceOnCOmpletedFlags, always no capture ExecutionContext and SynchronizationContext. */

            /*
                PatternA: GetStatus=Pending => OnCompleted => TrySet*** => GetResult
                PatternB: TrySet*** => GetStatus=!Pending => GetResult
                PatternC: GetStatus=Pending => TrySet/OnCompleted(race condition) => GetResult
                C.1: win OnCompleted -> TrySet invoke saved continuation
                C.2: win TrySet -> should invoke continuation here.
            */

            // not set continuation yet.
            object oldContinuation = this.continuation;
            if (oldContinuation == null)
            {
                oldContinuation = Interlocked.CompareExchange(ref this.continuation, continuation, null);
            }

            if (oldContinuation != null)
            {
                // already running continuation in TrySet.
                // It will cause call OnCompleted multiple time, invalid.
                if (!ReferenceEquals(oldContinuation, UniTaskCompletionSourceCoreShared.s_sentinel))
                {
                    throw new InvalidOperationException("Already continuation registered, can not await twice or get Status after await.");
                }

                continuation?.Invoke();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateToken(short token)
        {
            if (token != version)
            {
                throw new InvalidOperationException($"Token {token} version {version} is not matched, can not await twice or get Status after await.");
            }
        }
    }

    public struct PromiseTaskCompletionSourceCore<TResult>
    {
        TResult result;
        object error; // ExceptionHolder or OperationCanceledException
        short version;
        bool hasUnhandledError;
        int completedCount; // 0: completed == false
        Action continuation;

        /// <summary>Gets the operation version.</summary>
        public short Version => version;



        public void Reset()
        {
            ReportUnhandledError();

            unchecked
            {
                version += 1; // incr version.
            }
            completedCount = 0;
            result = default;
            error = null;
            hasUnhandledError = false;
            continuation = null;
        }

        void ReportUnhandledError()
        {
            if (hasUnhandledError)
            {
                try
                {
                    if (error is OperationCanceledException oc)
                    {
                        //UniTaskScheduler.PublishUnobservedTaskException(oc);
                    }
                }
                catch
                {
                }
            }
        }

        internal void MarkHandled()
        {
            hasUnhandledError = false;
        }

        /// <summary>Completes with a successful result.</summary>
        /// <param name="result">The result.</param>

        public bool TrySetResult(TResult result)
        {
            if (Interlocked.Increment(ref completedCount) == 1)
            {
                // setup result
                this.result = result;

                if (continuation != null || Interlocked.CompareExchange(ref continuation, UniTaskCompletionSourceCoreShared.s_sentinel, null) != null)
                {
                    continuation?.Invoke();
                }
                return true;
            }

            return false;
        }

        /// <summary>Completes with an error.</summary>
        /// <param name="ex">The exception.</param>

        public bool TrySetException(Exception ex)
        {
            if (Interlocked.Increment(ref completedCount) == 1)
            {
                // setup result
                hasUnhandledError = true;
                if (ex is OperationCanceledException)
                {
                    this.error = ex;
                }
                else
                {
#if DEBUG
                    this.error = new ExceptionHolder            (ExceptionDispatchInfo.Capture(ex));
#else
                    this.error = ex;
#endif
                }

                if (continuation != null || Interlocked.CompareExchange(ref continuation, UniTaskCompletionSourceCoreShared.s_sentinel, null) != null)
                {
                    continuation?.Invoke();
                }
                return true;
            }

            return false;
        }

        public bool TrySetCanceled(CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref completedCount) == 1)
            {
                // setup result
                hasUnhandledError = true;
                error = new OperationCanceledException(cancellationToken);

                if (continuation != null || Interlocked.CompareExchange(ref continuation, UniTaskCompletionSourceCoreShared.s_sentinel, null) != null)
                {
                    continuation?.Invoke();
                }
                return true;
            }

            return false;
        }


        /// <summary>Gets the status of the operation.</summary>
        /// <param name="token">Opaque value that was provided to the <see cref="UniTask"/>'s constructor.</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromiseTaskStatus GetStatus(short token)
        {
            ValidateToken(token);
            return continuation == null || completedCount == 0 ? PromiseTaskStatus.Pending
                 : error == null ? PromiseTaskStatus.Succeeded
                 : error is OperationCanceledException ? PromiseTaskStatus.Canceled
                 : PromiseTaskStatus.Faulted;
        }

        /// <summary>Gets the status of the operation without token validation.</summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PromiseTaskStatus UnsafeGetStatus()
        {
            return continuation == null || completedCount == 0 ? PromiseTaskStatus.Pending
                 : error == null ? PromiseTaskStatus.Succeeded
                 : error is OperationCanceledException ? PromiseTaskStatus.Canceled
                 : PromiseTaskStatus.Faulted;
        }

        /// <summary>Gets the result of the operation.</summary>
        /// <param name="token">Opaque value that was provided to the <see cref="UniTask"/>'s constructor.</param>
        // [StackTraceHidden]

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult GetResult(short token)
        {
            ValidateToken(token);
            if (completedCount == 0)
            {
                throw new InvalidOperationException("Not yet completed, UniTask only allow to use await.");
            }

            if (error != null)
            {
                hasUnhandledError = false;
                if (error is OperationCanceledException oce)
                {
                    throw oce;
                }
#if DEBUG
                else if (error is ExceptionHolder eh)
                {
                    eh.GetException().Throw();
                }
#else
                else if (error is Exception ex)
                {
                    throw ex;
                }
#endif
                throw new InvalidOperationException("Critical: invalid exception type was held.");
            }

            return result;
        }

        /// <summary>Schedules the continuation action for this operation.</summary>
        /// <param name="continuation">The continuation to invoke when the operation has completed.</param>
        /// <param name="token">Opaque value that was provided to the <see cref="UniTask"/>'s constructor.</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Action continuation, short token /*, ValueTaskSourceOnCompletedFlags flags */)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }
            ValidateToken(token);

            /* no use ValueTaskSourceOnCOmpletedFlags, always no capture ExecutionContext and SynchronizationContext. */

            /*
                PatternA: GetStatus=Pending => OnCompleted => TrySet*** => GetResult
                PatternB: TrySet*** => GetStatus=!Pending => GetResult
                PatternC: GetStatus=Pending => TrySet/OnCompleted(race condition) => GetResult
                C.1: win OnCompleted -> TrySet invoke saved continuation
                C.2: win TrySet -> should invoke continuation here.
            */

            // not set continuation yet.
            object oldContinuation = this.continuation;
            if (oldContinuation == null)
            {
                oldContinuation = Interlocked.CompareExchange(ref this.continuation, continuation, null);
            }

            if (oldContinuation != null)
            {
                // already running continuation in TrySet.
                // It will cause call OnCompleted multiple time, invalid.
                if (!ReferenceEquals(oldContinuation, UniTaskCompletionSourceCoreShared.s_sentinel))
                {
                    throw new InvalidOperationException("Already continuation registered, can not await twice or get Status after await.");
                }

                continuation?.Invoke();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateToken(short token)
        {
            if (token != version)
            {
                throw new InvalidOperationException($"Token {token} version {version} is not matched, can not await twice or get Status after await.");
            }
        }
    }

}