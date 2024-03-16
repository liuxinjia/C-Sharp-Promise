using RSG.Promises;
using System;
using System.Collections.Generic;
using System.Linq;
using RSG.Exceptions;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace RSG
{
    /// <summary>
    /// Implements a C# promise.
    /// https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise
    /// </summary>
    public interface IPromise<PromisedT>
    {
        /// <summary>
        /// Gets the id of the promise, useful for referencing the promise during runtime.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Set the name of the promise, useful for debugging.
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
        /// Complete the promise. Adds a default error handler.
        /// </summary>
        void Done();

        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        IPromise Catch(Action<Exception> onRejected);

        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        IPromise<PromisedT> Catch(Func<Exception, PromisedT> onRejected);

        /// <summary>
        /// Add a resolved callback that chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, IPromise<ConvertedT>> onResolved);

        /// <summary>
        /// Add a resolved callback that chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved);

        /// <summary>
        /// Add a resolved callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved);

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected
        );

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected);

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected,
            Action<float> onProgress
        );

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// </summary>
        IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected, Action<float> onProgress);

        /// <summary>
        /// Return a new promise with a different value.
        /// May also change the type of the value.
        /// </summary>
        IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, ConvertedT> transform);

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Returns a promise for a collection of the resolved results.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Converts to a non-value promise.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        IPromise ThenAll(Func<PromisedT, IEnumerable<IPromise>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        IPromise<ConvertedT> ThenRace<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain);

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a non-value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        IPromise ThenRace(Func<PromisedT, IEnumerable<IPromise>> chain);

        /// <summary> 
        /// Add a finally callback. 
        /// Finally callbacks will always be called, even if any preceding promise is rejected, or encounters an error.
        /// The returned promise will be resolved or rejected, as per the preceding promise.
        /// </summary> 
        IPromise<PromisedT> Finally(Action onComplete);

        /// <summary>
        /// Add a callback that chains a non-value promise.
        /// ContinueWith callbacks will always be called, even if any preceding promise is rejected, or encounters an error.
        /// The state of the returning promise will be based on the new non-value promise, not the preceding (rejected or resolved) promise.
        /// </summary>
        IPromise ContinueWith(Func<IPromise> onResolved);

        /// <summary> 
        /// Add a callback that chains a value promise (optionally converting to a different value type).
        /// ContinueWith callbacks will always be called, even if any preceding promise is rejected, or encounters an error.
        /// The state of the returning promise will be based on the new value promise, not the preceding (rejected or resolved) promise.
        /// </summary> 
        IPromise<ConvertedT> ContinueWith<ConvertedT>(Func<IPromise<ConvertedT>> onComplete);

        /// <summary>
        /// Add a progress callback.
        /// Progress callbacks will be called whenever the promise owner reports progress towards the resolution
        /// of the promise.
        /// </summary>
        IPromise<PromisedT> Progress(Action<float> onProgress);
    }

    /// <summary>
    /// Interface for a promise that can be rejected.
    /// </summary>
    public interface IRejectable
    {
        /// <summary>
        /// Reject the promise with an exception.
        /// </summary>
        void Reject(Exception ex);
    }

    /// <summary>
    /// Interface for a promise that can be rejected or resolved.
    /// </summary>
    public interface IPendingPromise<PromisedT> : IRejectable
    {
        /// <summary>
        /// ID of the promise, useful for debugging.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Resolve the promise with a particular value.
        /// </summary>
        void Resolve(PromisedT value);

        /// <summary>
        /// Report progress in a promise.
        /// </summary>
        void ReportProgress(float progress);
    }

    /// <summary>
    /// Specifies the state of a promise.
    /// </summary>
    public enum PromiseState
    {
        Pending,    // The promise is in-flight.
        Rejected,   // The promise has been rejected.
        Resolved    // The promise has been resolved.
    };

    /// <summary>
    /// Implements a C# promise.
    /// https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise
    /// </summary>

#if DEBUG
    public class Promise<PromisedT> : IPromise<PromisedT>, IPendingPromise<PromisedT>, IPromiseInfo
#else
    public struct Promise<PromisedT> : IPromise<PromisedT>, IPendingPromise<PromisedT>, IPromiseInfo
#endif
    {
        /// <summary>
        /// Error handler.
        /// </summary>
        private List<RejectHandler> rejectHandlers;

        /// <summary>
        /// Progress handlers.
        /// </summary>
        private List<ProgressHandler> progressHandlers;

        /// <summary>
        /// Completed handlers that accept a value.
        /// </summary>
        private List<Action<PromisedT>> resolveCallbacks;
        private List<IRejectable> resolveRejectables;

        /// <summary>
        /// ID of the promise, useful for debugging.
        /// </summary>
        public int Id { get { return id; } }

        private readonly int id;
        /// <summary>
        /// Name of the promise, when set, useful for debugging.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Tracks the current state of the promise.
        /// </summary>
        public PromiseState CurState
        {
            get
            {
                this.TryGetState(id, out var state);
                return state;
            }
            private set
            {
                this.ChangeState(id, value);
            }
        }

        /// <summary>
        /// The exception when the promise is rejected.
        /// </summary>
        private Exception rejectionException
        {
            get
            {
                this.TryGetEx(id, out var e);
                return e;
            }
            set
            {
                this.ChangeEx(id, value);
            }
        }

        /// <summary>
        /// The value when the promises is resolved.
        /// </summary>
        private PromisedT resolveValue
        {
            get
            {
                this.TryGetValue<PromisedT>(id, out var value);
                return value;
            }
            set
            {
                this.ChangeValue(id, value);
            }
        }


        public static Promise<PromisedT> Create(string name = null, PromiseState initialState = PromiseState.Pending)
        {
            var promise = new Promise<PromisedT>(name, initialState);
            return promise;
        }

        public Promise(Action<Action<PromisedT>, Action<Exception>> resolver) 
        {
            resolveCallbacks = new List<Action<PromisedT>>();
            resolveRejectables = new List<IRejectable>();
            rejectHandlers = new List<RejectHandler>();
            progressHandlers = new List<ProgressHandler>();
            Name = null;
            id = Promise.NextId();
            CurState = PromiseState.Pending;

            if (Promise.EnablePromiseTracking)
            {
                Promise.PendingPromises.Add(this.id);
            }

            try
            {
                resolver(Resolve, Reject);
            }
            catch (Exception ex)
            {
                Reject(ex);
            }
        }

        private Promise(string name = null, PromiseState initialState = PromiseState.Pending)
        {
            resolveCallbacks = new List<Action<PromisedT>>();
            resolveRejectables = new List<IRejectable>();
            rejectHandlers = new List<RejectHandler>();
            progressHandlers = new List<ProgressHandler>();
            Name = name;
            id = Promise.NextId();
            CurState = initialState;

            if (Promise.EnablePromiseTracking)
            {
                Promise.PendingPromises.Add(this.id);
            }
        }

        /// <summary>
        /// Add a rejection handler for this promise.
        /// </summary>
        private void AddRejectHandler(Action<Exception> onRejected, IRejectable rejectable)
        {
            if (rejectHandlers == null)
            {
                rejectHandlers = new List<RejectHandler>();
            }

            rejectHandlers.Add(new RejectHandler { callback = onRejected, rejectable = rejectable });
        }

        /// <summary>
        /// Add a resolve handler for this promise.
        /// </summary>
        private void AddResolveHandler(Action<PromisedT> onResolved, IRejectable rejectable)
        {
            if (resolveCallbacks == null)
            {
                resolveCallbacks = new List<Action<PromisedT>>();
            }

            if (resolveRejectables == null)
            {
                resolveRejectables = new List<IRejectable>();
            }

            resolveCallbacks.Add(onResolved);
            resolveRejectables.Add(rejectable);
        }

        /// <summary>
        /// Add a progress handler for this promise.
        /// </summary>
        private void AddProgressHandler(Action<float> onProgress, IRejectable rejectable)
        {
            if (progressHandlers == null)
            {
                progressHandlers = new List<ProgressHandler>();
            }

            progressHandlers.Add(new ProgressHandler { callback = onProgress, rejectable = rejectable });
        }

        /// <summary>
        /// Invoke a single handler.
        /// </summary>
        private void InvokeHandler<T>(Action<T> callback, IRejectable rejectable, T value)
        {
            //            Argument.NotNull(() => callback);
            //            Argument.NotNull(() => rejectable);            

            try
            {
                callback(value);
            }
            catch (Exception ex)
            {
                rejectable.Reject(ex);
            }
        }

        /// <summary>
        /// Helper function clear out all handlers after resolution or rejection.
        /// </summary>
        private void ClearHandlers()
        {
            rejectHandlers = null;
            resolveCallbacks = null;
            resolveRejectables = null;
            progressHandlers = null;
        }

        /// <summary>
        /// Invoke all reject handlers.
        /// </summary>
        private void InvokeRejectHandlers(Exception ex)
        {
            //            Argument.NotNull(() => ex);

            if (rejectHandlers != null)
            {
                for (int i = 0, maxI = rejectHandlers.Count; i < maxI; ++i)
                    InvokeHandler(rejectHandlers[i].callback, rejectHandlers[i].rejectable, ex);
            }

            ClearHandlers();
        }

        /// <summary>
        /// Invoke all resolve handlers.
        /// </summary>
        private void InvokeResolveHandlers(PromisedT value)
        {
            if (resolveCallbacks != null)
            {
                for (int i = 0, maxI = resolveCallbacks.Count; i < maxI; i++)
                {
                    InvokeHandler(resolveCallbacks[i], resolveRejectables[i], value);
                }
            }

            ClearHandlers();
        }

        /// <summary>
        /// Invoke all progress handlers.
        /// </summary>
        private void InvokeProgressHandlers(float progress)
        {
            if (progressHandlers != null)
            {
                for (int i = 0, maxI = progressHandlers.Count; i < maxI; ++i)
                    InvokeHandler(progressHandlers[i].callback, progressHandlers[i].rejectable, progress);
            }
        }

        /// <summary>
        /// Reject the promise with an exception.
        /// </summary>
        public void Reject(Exception ex)
        {
            //            Argument.NotNull(() => ex);

            if (CurState != PromiseState.Pending)
            {
                throw new PromiseStateException(
                    "Attempt to reject a promise that is already in state: " + CurState
                    + ", a promise can only be rejected when it is still in state: "
                    + PromiseState.Pending
                );
            }

            rejectionException = ex;
            CurState = PromiseState.Rejected;

            if (Promise.EnablePromiseTracking)
            {
                Promise.PendingPromises.Remove(this.id);
            }

            InvokeRejectHandlers(ex);
        }

        /// <summary>
        /// Resolve the promise with a particular value.
        /// </summary>
        public void Resolve(PromisedT value)
        {
            if (CurState != PromiseState.Pending)
            {
                throw new PromiseStateException(
                    "Attempt to resolve a promise that is already in state: " + CurState
                    + ", a promise can only be resolved when it is still in state: "
                    + PromiseState.Pending
                );
            }

            resolveValue = value;
            CurState = PromiseState.Resolved;

            if (Promise.EnablePromiseTracking)
            {
                Promise.PendingPromises.Remove(this.id);
            }

            InvokeResolveHandlers(value);
        }

        /// <summary>
        /// Report progress on the promise.
        /// </summary>
        public void ReportProgress(float progress)
        {
            if (CurState != PromiseState.Pending)
            {
                throw new PromiseStateException(
                    "Attempt to report progress on a promise that is already in state: "
                    + CurState + ", a promise can only report progress when it is still in state: "
                    + PromiseState.Pending
                );
            }

            InvokeProgressHandlers(progress);
        }

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// onRejected is called on error.
        /// </summary>
        public void Done(Action<PromisedT> onResolved, Action<Exception> onRejected)
        {
            Then(onResolved, onRejected)
#if DEBUG
                .Catch(ex =>
                    Promise.PropagateUnhandledException(this, ex)
                );
#else
                ;
#endif
        }

        /// <summary>
        /// Completes the promise. 
        /// onResolved is called on successful completion.
        /// Adds a default error handler.
        /// </summary>
        public void Done(Action<PromisedT> onResolved)
        {
            Then(onResolved)
#if DEBUG
                .Catch(ex =>
                    Promise.PropagateUnhandledException(this, ex)
                );
#else
                ;
#endif
        }

        /// <summary>
        /// Complete the promise. Adds a default error handler.
        /// </summary>
        public void Done()
        {
            if (CurState == PromiseState.Resolved)
                return;

#if DEBUG
            Catch(ex =>
                Promise.PropagateUnhandledException(this, ex)
            );
#endif
        }

        /// <summary>
        /// Set the name of the promise, useful for debugging.
        /// </summary>
        public IPromise<PromisedT> WithName(string name)
        {
#if DEBUG
            this.Name = name;
#endif
            return this;
        }


        /// <summary>
        /// Handle errors for the promise. 
        /// </summary>
        public IPromise Catch(Action<Exception> onRejected)
        {
            if (CurState == PromiseState.Resolved)
            {
                return Promise.Resolved();
            }

            var resultPromise = Promise.Create(Name);
#if DEBUG
            resultPromise.WithName(Name);
#endif

            Action<PromisedT> resolveHandler = _ => resultPromise.Resolve();

            Action<Exception> rejectHandler = ex =>
            {
                try
                {
                    onRejected(ex);
                    resultPromise.Resolve();
                }
                catch (Exception cbEx)
                {
                    resultPromise.Reject(cbEx);
                }
            };

            ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            ProgressHandlers(resultPromise, v => resultPromise.ReportProgress(v));

            return resultPromise;
        }

        /// <summary>
        /// Handle errors for the promise.
        /// </summary>
        public IPromise<PromisedT> Catch(Func<Exception, PromisedT> onRejected)
        {
            if (CurState == PromiseState.Resolved)
            {
                return this;
            }

            var resultPromise = Promise<PromisedT>.Create(this.Name);
#if DEBUG
            resultPromise.WithName(Name);
#endif
            Action<PromisedT> resolveHandler = v => resultPromise.Resolve(v);

            Action<Exception> rejectHandler = ex =>
            {
                try
                {
                    resultPromise.Resolve(onRejected(ex));
                }
                catch (Exception cbEx)
                {
                    resultPromise.Reject(cbEx);
                }
            };

            ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            ProgressHandlers(resultPromise, v => resultPromise.ReportProgress(v));

            return resultPromise;
        }

        /// <summary>
        /// Add a resolved callback that chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, IPromise<ConvertedT>> onResolved)
        {
            return Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback that chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<PromisedT, IPromise> onResolved)
        {
            return Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback.
        /// </summary>
        public IPromise Then(Action<PromisedT> onResolved)
        {
            return Then(onResolved, null, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected
        )
        {
            return Then(onResolved, onRejected, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }

        /// <summary>
        /// Add a resolved callback and a rejected callback.
        /// </summary>
        public IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }


        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a value promise (optionally converting to a different value type).
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(
            Func<PromisedT, IPromise<ConvertedT>> onResolved,
            Func<Exception, IPromise<ConvertedT>> onRejected,
            Action<float> onProgress
        )
        {
            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    return onResolved(resolveValue);
                }
                catch (Exception ex)
                {
                    return Promise<ConvertedT>.Rejected(ex, this.Name);
                }
            }

            // This version of the function must supply an onResolved.
            // Otherwise there is now way to get the converted value to pass to the resulting promise.
            //            Argument.NotNull(() => onResolved); 

            var resultPromise = Promise<ConvertedT>.Create(this.Name);
#if DEBUG
            resultPromise.WithName(Name);
#endif

            Action<PromisedT> resolveHandler = v =>
            {
                onResolved(v)
                    .Progress(progress => resultPromise.ReportProgress(progress))
                    .Then(
                        // Should not be necessary to specify the arg type on the next line, but Unity (mono) has an internal compiler error otherwise.
                        chainedValue => resultPromise.Resolve(chainedValue),
                        ex => resultPromise.Reject(ex)
                    );
            };

            Action<Exception> rejectHandler = ex =>
            {
                if (onRejected == null)
                {
                    resultPromise.Reject(ex);
                    return;
                }

                try
                {
                    onRejected(ex)
                        .Then(
                            chainedValue => resultPromise.Resolve(chainedValue),
                            callbackEx => resultPromise.Reject(callbackEx)
                        );
                }
                catch (Exception callbackEx)
                {
                    resultPromise.Reject(callbackEx);
                }
            };

            ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            if (onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// The resolved callback chains a non-value promise.
        /// </summary>
        public IPromise Then(Func<PromisedT, IPromise> onResolved, Action<Exception> onRejected, Action<float> onProgress)
        {
            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    return onResolved(resolveValue);
                }
                catch (Exception ex)
                {
                    return Promise.Rejected(ex, this.Name);
                }
            }

            var resultPromise = Promise.Create(this.Name);
#if DEBUG
            resultPromise.WithName(Name);
#endif

            Action<PromisedT> resolveHandler = v =>
            {
                if (onResolved != null)
                {
                    onResolved(v)
                        .Progress(progress => resultPromise.ReportProgress(progress))
                        .Then(
                            () => resultPromise.Resolve(),
                            ex => resultPromise.Reject(ex)
                        );
                }
                else
                {
                    resultPromise.Resolve();
                }
            };

            Action<Exception> rejectHandler;
            if (onRejected != null)
            {
                rejectHandler = ex =>
                {
                    onRejected(ex);
                    resultPromise.Reject(ex);
                };
            }
            else
            {
                rejectHandler = resultPromise.Reject;
            }

            ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            if (onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        /// <summary>
        /// Add a resolved callback, a rejected callback and a progress callback.
        /// </summary>
        public IPromise Then(Action<PromisedT> onResolved, Action<Exception> onRejected, Action<float> onProgress)
        {
            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    onResolved(resolveValue);
                    return Promise.Resolved();
                }
                catch (Exception ex)
                {
                    return Promise.Rejected(ex, this.Name);
                }
            }

            var resultPromise = Promise.Create(this.Name);
#if DEBUG   
            resultPromise.WithName(Name);
#endif

            Action<PromisedT> resolveHandler = v =>
            {
                if (onResolved != null)
                {
                    onResolved(v);
                }

                resultPromise.Resolve();
            };

            Action<Exception> rejectHandler;
            if (onRejected != null)
            {
                rejectHandler = ex =>
                {
                    onRejected(ex);
                    resultPromise.Reject(ex);
                };
            }
            else
            {
                rejectHandler = resultPromise.Reject;
            }

            ActionHandlers(resultPromise, resolveHandler, rejectHandler);
            if (onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        /// <summary>
        /// Return a new promise with a different value.
        /// May also change the type of the value.
        /// </summary>
        public IPromise<ConvertedT> Then<ConvertedT>(Func<PromisedT, ConvertedT> transform)
        {
            //            Argument.NotNull(() => transform);
            string name = Name;
            return Then(value => Promise<ConvertedT>.Resolved(transform(value), name));
        }

        /// <summary>
        /// Helper function to invoke or register resolve/reject handlers.
        /// </summary>
        private void ActionHandlers(IRejectable resultPromise, Action<PromisedT> resolveHandler, Action<Exception> rejectHandler)
        {
            if (CurState == PromiseState.Resolved)
            {
                InvokeHandler(resolveHandler, resultPromise, resolveValue);
            }
            else if (CurState == PromiseState.Rejected)
            {
                InvokeHandler(rejectHandler, resultPromise, rejectionException);
            }
            else
            {
                AddResolveHandler(resolveHandler, resultPromise);
                AddRejectHandler(rejectHandler, resultPromise);
            }
        }

        /// <summary>
        /// Helper function to invoke or register progress handlers.
        /// </summary>
        private void ProgressHandlers(IRejectable resultPromise, Action<float> progressHandler)
        {
            if (CurState == PromiseState.Pending)
            {
                AddProgressHandler(progressHandler, resultPromise);
            }
        }

        /// <summary>
        /// Chain a number of operations using promises.
        /// Returns the value of the first promise that resolves, or otherwise the exception thrown by the last operation.
        /// </summary>
        public static IPromise<T> First<T>(string name, params Func<IPromise<T>>[] fns)
        {
            return First(name, (IEnumerable<Func<IPromise<T>>>)fns);
        }

        /// <summary>
        /// Chain a number of operations using promises.
        /// Returns the value of the first promise that resolves, or otherwise the exception thrown by the last operation.
        /// </summary>
        public static IPromise<T> First<T>(string name, IEnumerable<Func<IPromise<T>>> fns)
        {
            var promise = Promise<T>.Create(name);

            int count = 0;

            fns.Aggregate(
                Promise<T>.Rejected(null, name),
                (prevPromise, fn) =>
                {
                    int itemSequence = count;
                    ++count;

                    var newPromise = Promise<T>.Create(name);
                    prevPromise
                        .Progress(v =>
                        {
                            var sliceLength = 1f / count;
                            promise.ReportProgress(sliceLength * (v + itemSequence));
                        })
                        .Then((Action<T>)newPromise.Resolve)
                        .Catch(ex =>
                        {
                            var sliceLength = 1f / count;
                            promise.ReportProgress(sliceLength * itemSequence);

                            fn()
                                .Then(value => newPromise.Resolve(value))
                                .Catch(newPromise.Reject)
                                .Done()
                            ;
                        })
                    ;
                    return newPromise;
                })
            .Then(value => promise.Resolve(value))
            .Catch(ex =>
            {
                promise.ReportProgress(1f);
                promise.Reject(ex);
            });

            return promise;
        }

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Returns a promise for a collection of the resolved results.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        public IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain)
        {
            string name = Name;
            return Then(value => Promise<ConvertedT>.All(name, chain(value)));
        }

        /// <summary>
        /// Chain an enumerable of promises, all of which must resolve.
        /// Converts to a non-value promise.
        /// The resulting promise is resolved when all of the promises have resolved.
        /// It is rejected as soon as any of the promises have been rejected.
        /// </summary>
        public IPromise ThenAll(Func<PromisedT, IEnumerable<IPromise>> chain)
        {
            string name = Name;
            return Then(value => Promise.All(name, chain(value)));
        }

        /// <summary>
        /// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        /// Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise<IEnumerable<PromisedT>> All(string name, params IPromise<PromisedT>[] promises)
        {
            return All(name, (IEnumerable<IPromise<PromisedT>>)promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        /// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        /// Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise<IEnumerable<PromisedT>> All(string name, IEnumerable<IPromise<PromisedT>> promises)
        {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0)
            {
                return Promise<IEnumerable<PromisedT>>.Resolved(Enumerable.Empty<PromisedT>(), name);
            }

            var remainingCount = promisesArray.Length;
            var results = new PromisedT[remainingCount];
            var progress = new float[remainingCount];
            var resultPromise = Promise<IEnumerable<PromisedT>>.Create(name);
#if DEBUG
            resultPromise.WithName("All");
#endif
            promisesArray.Each((promise, index) =>
            {
                promise
                    .Progress(v =>
                    {
                        progress[index] = v;
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            resultPromise.ReportProgress(progress.Average());
                        }
                    })
                    .Then(result =>
                    {
                        progress[index] = 1f;
                        results[index] = result;

                        --remainingCount;
                        if (remainingCount <= 0 && resultPromise.CurState == PromiseState.Pending)
                        {
                            // This will never happen if any of the promises errorred.
                            resultPromise.Resolve(results);
                        }
                    })
                    .Catch(ex =>
                    {
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            // If a promise errorred and the result promise is still pending, reject it.
                            resultPromise.Reject(ex);
                        }
                    })
                    .Done();
            });

            return resultPromise;
        }

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        public IPromise<ConvertedT> ThenRace<ConvertedT>(Func<PromisedT, IEnumerable<IPromise<ConvertedT>>> chain)
        {
            string name = Name;
            return Then(value => Promise<ConvertedT>.Race(name, chain(value)));
        }

        /// <summary>
        /// Takes a function that yields an enumerable of promises.
        /// Converts to a non-value promise.
        /// Returns a promise that resolves when the first of the promises has resolved.
        /// Yields the value from the first promise that has resolved.
        /// </summary>
        public IPromise ThenRace(Func<PromisedT, IEnumerable<IPromise>> chain)
        {
            string name = Name;
            return Then(value => Promise.Race(name, chain(value)));
        }

        /// <summary>
        /// Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        /// Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise<PromisedT> Race(string name, params IPromise<PromisedT>[] promises)
        {
            return Race(name, (IEnumerable<IPromise<PromisedT>>)promises); // Cast is required to force use of the other function.
        }

        /// <summary>
        /// Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        /// Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise<PromisedT> Race(string name, IEnumerable<IPromise<PromisedT>> promises)
        {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0)
            {
                throw new InvalidOperationException(
                    "At least 1 input promise must be provided for Race"
                );
            }

            var resultPromise = Promise<PromisedT>.Create(name);
#if DEBUG
            resultPromise.WithName("Race");
#endif
            var progress = new float[promisesArray.Length];

            promisesArray.Each((promise, index) =>
            {
                promise
                    .Progress(v =>
                    {
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            progress[index] = v;
                            resultPromise.ReportProgress(progress.Max());
                        }
                    })
                    .Then(result =>
                    {
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            resultPromise.Resolve(result);
                        }
                    })
                    .Catch(ex =>
                    {
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            // If a promise errorred and the result promise is still pending, reject it.
                            resultPromise.Reject(ex);
                        }
                    })
                    .Done();
            });

            return resultPromise;
        }

        /// <summary>
        /// Convert a simple value directly into a resolved promise.
        /// </summary>
        public static IPromise<PromisedT> Resolved(PromisedT promisedValue, string name = null)
        {
            var promise = Promise<PromisedT>.Create(name, PromiseState.Resolved);
            promise.resolveValue = promisedValue;
            return promise;
        }

        /// <summary>
        /// Convert an exception directly into a rejected promise.
        /// </summary>
        public static IPromise<PromisedT> Rejected(Exception ex, string name = null)
        {
            //            Argument.NotNull(() => ex);

            var promise = Promise<PromisedT>.Create(name, PromiseState.Rejected);
            promise.rejectionException = ex;
            return promise;
        }

        public IPromise<PromisedT> Finally(Action onComplete)
        {
            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    onComplete();
                    return this;
                }
                catch (Exception ex)
                {
                    return Rejected(ex, this.Name);
                }
            }

            var promise = Promise<PromisedT>.Create(this.Name);
#if DEBUG
            promise.WithName(Name);
#endif
            this.Then((Action<PromisedT>)promise.Resolve);
            this.Catch(e =>
            {
                try
                {
                    onComplete();
                    promise.Reject(e);
                }
                catch (Exception ne)
                {
                    promise.Reject(ne);
                }
            });

            return promise.Then(v =>
            {
                onComplete();
                return v;
            });
        }

        public IPromise ContinueWith(Func<IPromise> onComplete)
        {
            var promise = Promise.Create(this.Name);
#if DEBUG
            promise.WithName(Name);
#endif

            this.Then(x => promise.Resolve());
            this.Catch(e => promise.Resolve());

            return promise.Then(onComplete);
        }

        public IPromise<ConvertedT> ContinueWith<ConvertedT>(Func<IPromise<ConvertedT>> onComplete)
        {
            var promise = Promise.Create(this.Name);
#if DEBUG
            promise.WithName(Name);
#endif

            this.Then(x => promise.Resolve());
            this.Catch(e => promise.Resolve());

            return promise.Then(onComplete);
        }

        public IPromise<PromisedT> Progress(Action<float> onProgress)
        {
            if (CurState == PromiseState.Pending && onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }
            return this;
        }
    }
}
