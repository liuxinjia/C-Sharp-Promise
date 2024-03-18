using System;
using System.Collections.Generic;

namespace RSG.Promises
{
    /// <summary>
    /// Represents a C# promise.
    /// See: https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise
    /// </summary>
    public interface IPromise<PromisedT> : IPendingPromise<PromisedT>, IBasePromise
    {

        /// <summary>
        /// Sets the name of the promise, useful for debugging.
        /// </summary>
        IPromise<PromisedT> WithName(string name);

        /// <summary>
        /// Completes the promise.
        /// onResolved is called on successful completion.
        /// onRejected is called on error.
        /// </summary>
        void Done(Action<PromisedT> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Completes the promise.
        /// onResolved is called on successful completion.
        /// Adds a default error handler.
        /// </summary>
        void Done(Action<PromisedT> onResolved);

        /// <summary>
        /// Handles errors for the promise.
        /// </summary>
        IPromise Catch(Action<Exception> onRejected);

        /// <summary>
        /// Handles errors for the promise.
        /// </summary>
        IPromise<PromisedT> Catch(Func<Exception, PromisedT> onRejected);

        /// <summary>
        /// Adds a resolved callback that chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, IPromise<ConvertedT>> onResolved);

        /// <summary>
        /// Adds a resolved callback that chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved);

        /// <summary>
        /// Adds a resolved callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved);

        /// <summary>
        /// Adds a resolved callback and a rejected callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected
        );

        /// <summary>
        /// Adds a resolved callback and a rejected callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Adds a resolved callback and a rejected callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Adds a resolved callback, a rejected callback, and a progress callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected,
            Action<float> onProgress
        );

        /// <summary>
        /// Adds a resolved callback, a rejected callback, and a progress callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Adds a resolved callback, a rejected callback, and a progress callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Returns a new promise with a different value.
        /// May also change the type of the value.
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, ConvertedT> transform);

        /// <summary>
        /// Chains an enumerable of promises, all of which must resolve.
        /// Returns a promise for a collection of the resolved results.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Chains an enumerable of promises, all of which must resolve.
        /// Converts to a non-value promise.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise ThenAll(Func<PromisedT, IEnumerable<IPromise>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// It is rejected until all of the promises have been rejected.
        /// </summary>
        IPromise<ConvertedT> ThenAny<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a non-value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// It is rejected until all of the promises have been rejected.
        /// </summary>
        IPromise ThenAny(Func<PromisedT, IEnumerable<IPromise>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise<ConvertedT> ThenRace<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a non-value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise ThenRace(Func<PromisedT, IEnumerable<IPromise>> chain);

        /// <summary>
        /// Chains a number of operations using promises.
        /// Returns the value of the first promise that resolves, or otherwise the exception thrown by the last operation.
        /// </summary>
        IPromise<ConvertedT> ThenFirst<ConvertedT>(IEnumerable<Func<IPromise<ConvertedT>>> fns);

        /// <summary>
        /// Adds a finally callback.
        /// Finally callbacks will always be called, even if any preceding promise is rejected or encounters an error.
        /// The returned promise will be resolved or rejected, as per the preceding promise.
        /// </summary>
        IPromise<PromisedT> Finally(Action onComplete);

        /// <summary>
        /// Adds a callback that chains a non-value promise.
        /// ContinueWith callbacks will always be called, even if any preceding promise is rejected or encounters an error.
        /// The state of the returning promise will be based on the new non-value promise, not the preceding (rejected or resolved) promise.
        /// </summary>
        IPromise ContinueWith(Func<IPromise> onResolved);

        /// <summary>
        /// Adds a callback that chains a non-value promise.
        /// ContinueWith callbacks will always be called, even if any preceding promise is rejected or encounters an error.
        /// The state of the returning promise will be based on the new non-value promise, not the preceding (rejected or resolved) promise.
        /// </summary>
        IPromise ContinueWith(Action onComplete);
        
        /// <summary>
        /// Adds a callback that chains a value promise (optionally converting to a different value type).
        /// ContinueWith callbacks will always be called, even if any preceding promise is rejected or encounters an error.
        /// The state of the returning promise will be based on the new value promise, not the preceding (rejected or resolved) promise.
        /// </summary>
        IPromise<ConvertedT> ContinueWith<ConvertedT>(Func<IPromise<ConvertedT>> onComplete);

        /// <summary>
        /// Adds a progress callback.
        /// Progress callbacks will be called whenever the promise owner reports progress towards the resolution of the promise.
        /// </summary>
        IPromise<PromisedT> Progress(Action<float> onProgress);
    }
}
