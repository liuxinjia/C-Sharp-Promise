using System;
using System.Collections.Generic;
using System.Linq;

namespace RSG.Promises
{
    public class Promise : IPromise
    {
        #region Static Fields
        private static Queue<List<ResolveHandler>> _resolveListPool = new Queue<List<ResolveHandler>>();
        private static Queue<List<RejectHandler>> _rejectListPool = new Queue<List<RejectHandler>>();
        private static Queue<List<ProgressHandler>> _progressListPool = new Queue<List<ProgressHandler>>();
        /// <summary>
        ///     Set to true to enable tracking of promises.
        /// </summary>
        public static bool EnablePromiseTracking = false;

        private static EventHandler<ExceptionEventArgs> _unhandledException;
        /// <summary>
        ///     Event raised for unhandled errors.
        ///     For this to work you have to complete your promises with a call to Done().
        /// </summary>
        public static event EventHandler<ExceptionEventArgs> UnhandledException
        {
            add { _unhandledException += value; }
            remove { _unhandledException -= value; }
        }

        /// <summary>
        ///     Id for the next promise that is created.
        /// </summary>
        private static int _nextPromiseId;

        /// <summary>
        ///     Information about pending promises.
        /// </summary>
        internal static readonly HashSet<IPromiseInfo> PendingPromises =
            new HashSet<IPromiseInfo>();

        // for unit-test
        public static int GetPendingPromiseCount()
        {
            return PendingPromises.Count;
        }

        public static void ClearPending()
        {
            PendingPromises.Clear();
        }
        /// <summary>
        ///     Convert a simple value directly into a resolved promise.
        /// </summary>
        private static readonly Promise ResolvedPromise = new Promise(PromiseState.Resolved);
        #endregion

        #region Fields
        /// <summary>
        ///     The exception when the promise is rejected.
        /// </summary>
        private Exception _rejectionException;

        /// <summary>
        ///     Error handlers.
        /// </summary>
        protected List<RejectHandler> _rejectHandlers;
        /// <summary>
        ///     Completed handlers that accept no value.
        /// </summary>
        protected List<ResolveHandler> _resolveHandlers;
        /// <summary>
        ///     Progress handlers.
        /// </summary>
        protected List<ProgressHandler> _progressHandlers;



        #endregion

        #region Properties
        public int Id { get; }
        public string Name { get; protected set; }
        public PromiseState CurState { get; protected set; }
        #endregion


        public Promise()
        {
            CurState = PromiseState.Pending;
            Id = NextId();


            if (EnablePromiseTracking)
            {
                PendingPromises.Add(this);
            }
        }

        public Promise(Action<Action, Action<Exception>> resolver) : this()
        {
            try
            {
                resolver(Resolve, RejectWithoutDebug);
            }
            catch (Exception ex)
            {
                Reject(ex);
            }
        }

        private Promise(PromiseState initialState) : this()
        {
            CurState = initialState;
            Id = NextId();
        }

        #region IPromiseInfo Implementation
        public IPromise WithName(string name)
        {
            Name = name;
            return this;
        }

        public virtual void Dispose()
        {
            ClearHandlers();
            Name = string.Empty;
            CurState = PromiseState.Pending;
        }

        internal static int NextId()
        {
            return ++_nextPromiseId;
        }
        #endregion

        #region IPromise
        public void Done(Action onResolved, Action<Exception> onRejected)
        {
            Then(onResolved, onRejected)
                .Catch(ex =>
                    PropagateUnhandledException(this, ex)
                );
        }

        public void Done(Action onResolved)
        {
            Then(onResolved)
                .Catch(ex =>
                    PropagateUnhandledException(this, ex)
                );
        }

        public virtual void Done()
        {
            Catch(ex => PropagateUnhandledException(this, ex));
        }

        public IPromise Catch(Action<Exception> onRejected)
        {


            if (CurState == PromiseState.Resolved)
            {
                return this;
            }
            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    onRejected(_rejectionException);
                    return Promise.Resolved();
                }
                catch (Exception cbEx)
                {
                    Promise.Rejected(cbEx);
                }
            }
            var resultPromise = GetRawPromise();
            resultPromise.WithName(Name);


            void RejectHandler(Exception ex)
            {
                try
                {
                    onRejected(ex);
                    resultPromise.Resolve();
                }
                catch (Exception callbackException)
                {
                    resultPromise.Reject(callbackException);
                }
            }

            ActionHandlers(resultPromise, resultPromise.Resolve, RejectHandler);
            ProgressHandlers(this, resultPromise.ReportProgress);

            return resultPromise;
        }

        public IPromise Then(Action onResolved)
        {
            return Then(onResolved, null, null);
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }

        public IPromise Then(Action onResolved, Action<Exception> onRejected, Action<float> onProgress)
        {
            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    onResolved();
                    return this;
                }
                catch (Exception ex)
                {
                    return Rejected(ex);
                }
            }
            if (CurState == PromiseState.Rejected)
            {
                try
                {
                    if (onRejected != null)
                    {
                        onRejected(_rejectionException);
                        return Promise.Resolved();
                    }
                    else
                    {
                        return Promise.Rejected(_rejectionException);
                    }
                }
                catch (Exception ex)
                {
                    return Promise.Rejected(ex);
                }
            }

            var resultPromise = GetRawPromise();
            resultPromise.WithName(Name);

            Action resolveHandler;
            if (onResolved != null)
            {
                resolveHandler = () =>
                {
                    onResolved();
                    resultPromise.Resolve();
                };
            }
            else
            {
                resolveHandler = resultPromise.Resolve;
            }

            Action<Exception> rejectHandler;
            if (onRejected != null)
            {
                rejectHandler = ex =>
                {
                    onRejected(ex);
                    // equal to catch if defined OnRejected
                    // we will catch the exception but still go on next operation of promise chain
                    resultPromise.Resolve();
                };
            }
            else
            {
                rejectHandler = resultPromise.RejectWithoutDebug;
            }

            ActionHandlers(resultPromise, resolveHandler, rejectHandler);

            if (onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        public IPromise Then(Func<IPromise> onResolved)
        {
            return Then(onResolved, null, null);
        }

        public IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected)
        {
            return Then(onResolved, onRejected, null);
        }

        public IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected, Action<float> onProgress)
        {
            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    return onResolved();
                }
                catch (Exception ex)
                {
                    return Rejected(ex);
                }
            }
            if (CurState == PromiseState.Rejected)
            {
                try
                {
                    if (onRejected != null)
                    {
                        onRejected(_rejectionException);
                        return Promise.Resolved();
                    }
                    else
                    {
                        return Promise.Rejected(_rejectionException);
                    }
                }
                catch (Exception ex)
                {
                    return Promise.Rejected(ex);
                }
            }
            var resultPromise = GetRawPromise();
            resultPromise.WithName(Name);

            Action resolveHandler;
            if (onResolved != null)
            {
                resolveHandler = () =>
                {
                    onResolved()
                        .Progress(progress => resultPromise.ReportProgress(progress))
                        .Then(
                            resultPromise.Resolve,
                            resultPromise.RejectWithoutDebug
                        );
                };
            }
            else
            {
                resolveHandler = resultPromise.Resolve;
            }

            Action<Exception> rejectHandler;
            if (onRejected != null)
            {
                rejectHandler = ex =>
                {
                    onRejected(ex);
                    // we will catch the exception but still go on next operation of promise chain
                    resultPromise.Resolve();
                };
            }
            else
            {
                rejectHandler = resultPromise.RejectWithoutDebug;
            }

            ActionHandlers(resultPromise, resolveHandler, rejectHandler);

            if (onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        public IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved)
        {
            return Then(onResolved, null, null);
        }

        public IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved, Func<Exception, IPromise<ConvertedT>> onRejected, Action<float> onProgress)
        {
            // This version of the function must supply an onResolved.
            // Otherwise there is now way to get the converted value to pass to the resulting promise.


            if (CurState == PromiseState.Resolved)
            {
                try
                {
                    return onResolved.Invoke();
                }
                catch (Exception ex)
                {
                    if (onRejected == null)
                    {
                        return onRejected?.Invoke(ex);
                    }
                    else
                    {
                        return Promise<ConvertedT>.Rejected(ex);
                    }
                }
            }
            if (CurState == PromiseState.Rejected)
            {
                try
                {
                    if (onRejected != null)
                    {
                        return onRejected.Invoke(_rejectionException);
                    }
                    else
                    {
                        return Promise<ConvertedT>.Rejected(_rejectionException);
                    }
                }
                catch (Exception ex)
                {
                    return Promise<ConvertedT>.Rejected(ex);
                }
            }
            var resultPromise = GetRawPromise<ConvertedT>();
            resultPromise.WithName(Name);

            void ResolveHandler() => onResolved()
                .Progress(resultPromise.ReportProgress)
                .Then(resultPromise.Resolve, resultPromise.RejectWithoutDebug);

            void RejectHandler(Exception ex)
            {
                if (onRejected == null)
                {
                    resultPromise.RejectWithoutDebug(ex);
                    return;
                }

                try
                {
                    onRejected(ex).Then(resultPromise.Resolve, resultPromise.RejectWithoutDebug);
                }
                catch (Exception callbackEx)
                {
                    resultPromise.Reject(callbackEx);
                }
            }

            ActionHandlers(resultPromise, ResolveHandler, RejectHandler);
            if (onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }

            return resultPromise;
        }

        public IPromise ThenAll(Func<IEnumerable<IPromise>> chain)
        {
            return Then(() => AllInternal(chain()));
        }

        public IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain)
        {
            return Then(() => Promise<ConvertedT>.All(chain()));
        }

        public IPromise ThenAny(Func<IEnumerable<IPromise>> chain)
        {
            return Then(() => AnyInternal(chain()));
        }

        public IPromise<ConvertedT> ThenAny<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain)
        {
            return Then(() => Promise<ConvertedT>.Any(chain()));
        }

        public IPromise ThenSequence(Func<IEnumerable<Func<IPromise>>> chain)
        {
            return Then(() => SequenceInternal(chain()));
        }

        public IPromise ThenRace(Func<IEnumerable<IPromise>> chain)
        {
            return Then(() => RaceInternal(chain()));
        }

        public IPromise<ConvertedT> ThenRace<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain)
        {
            return Then(() => Promise<ConvertedT>.Race(chain()));
        }

        public IPromise Finally(Action onComplete)
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
                    return Rejected(ex);
                }
            }

            var promise = GetRawPromise();
            promise.WithName(Name);

            Then(promise.Resolve);
            Catch(e =>
            {
                // Things different from continue with
                // since we need to handle exception at last
                try
                {
                    onComplete();
                    promise.RejectWithoutDebug(e);
                }
                catch (Exception ne)
                {
                    promise.Reject(ne);
                }
            });

            return promise.Then(onComplete);
        }

        public IPromise ContinueWith(Func<IPromise> onComplete)
        {
            var promise = GetRawPromise();
            promise.WithName(Name);

            Then(promise.Resolve);
            Catch(_ => promise.Resolve());
            return promise.Then(onComplete);
        }

        public IPromise<ConvertedT> ContinueWith<ConvertedT>(Func<IPromise<ConvertedT>> onComplete)
        {
            Promise promise = GetRawPromise();
            promise.WithName(Name);

            Then(promise.Resolve);
            Catch(_ => promise.Resolve());
            return promise.Then(onComplete);
        }

        public IPromise Progress(Action<float> onProgress)
        {
            if (CurState == PromiseState.Pending && onProgress != null)
            {
                ProgressHandlers(this, onProgress);
            }
            return this;
        }
        #endregion

        #region IPendingPromise
        public void Resolve()
        {
            if (CurState != PromiseState.Pending)
            {
                throw new Exception(PromiseExceptionType.Valid_RESOLVED_STATE.ToString());
            }

            CurState = PromiseState.Resolved;
            if (EnablePromiseTracking)
            {
                PendingPromises.Remove(this);
            }

            InvokeResolveHandlers();
        }

        public void ReportProgress(float progress)
        {
            if (CurState != PromiseState.Pending)
            {
                throw new Exception(PromiseExceptionType.Valid_PROGRESS_STATE.ToString());
            }

            InvokeProgressHandlers(progress);
        }

        public void Reject(Exception ex)
        {
            RejectWithoutDebug(ex);

        }

        public void RejectWithoutDebug(Exception ex)
        {

            if (CurState != PromiseState.Pending)
            {
                throw new Exception(PromiseExceptionType.Valid_REJECTED_STATE.ToString());
            }

            _rejectionException = ex;
            CurState = PromiseState.Rejected;

            if (EnablePromiseTracking)
            {
                PendingPromises.Remove(this);
            }

            // only output error when you don't achieve onRejected
            InvokeRejectHandlers(ex);
        }

        public void Cancel()
        {
            CurState = PromiseState.Pending;

            if (Promise.EnablePromiseTracking)
            {
                Promise.PendingPromises.Remove(this);
            }

            ClearHandlers();
        }
        #endregion

        #region private methods
        // Helper Function to invoke or register resolve/reject handlers
        protected void ActionHandlers(IRejectable resultPromise, Action resolveHandler, Action<Exception> rejectHandler)
        {
            switch (CurState)
            {
                case PromiseState.Resolved:
                    InvokeResolveHandler(resolveHandler, resultPromise);
                    break;
                case PromiseState.Rejected:
                    InvokeRejectHandler(rejectHandler, resultPromise, _rejectionException);
                    break;
                default:
                    AddResolveHandler(resolveHandler, resultPromise);
                    AddRejectHandler(rejectHandler, resultPromise);
                    break;
            }
        }

        // Helper function to invoke or register progress handlers.
        protected void ProgressHandlers(IRejectable resultPromise, Action<float> progressHandler)
        {
            if (CurState == PromiseState.Pending)
            {
                AddProgressHandler(progressHandler, resultPromise);
            }
        }

        private void AddRejectHandler(Action<Exception> onRejected, IRejectable rejectable)
        {
            if (_rejectHandlers == null)
            {
                if (_rejectListPool.Count == 0)
                {
                    _rejectHandlers = new List<RejectHandler>();
                }
                else
                {
                    _rejectHandlers = _rejectListPool.Dequeue();
                }
            }
            _rejectHandlers.Add(new RejectHandler
            {
                Callback = onRejected,
                Rejectable = rejectable
            });
        }

        private void AddResolveHandler(Action onResolved, IRejectable rejectable)
        {
            if (_resolveHandlers == null)
            {
                if (_resolveListPool.Count == 0)
                {
                    _resolveHandlers = new List<ResolveHandler>();
                }
                else
                {
                    _resolveHandlers = _resolveListPool.Dequeue();
                }
            }
            _resolveHandlers.Add(new ResolveHandler
            {
                Callback = onResolved,
                Rejectable = rejectable
            });
        }

        private void AddProgressHandler(Action<float> onProgress, IRejectable rejectable)
        {
            if (_progressHandlers == null)
            {
                if (_progressListPool.Count == 0)
                {
                    _progressHandlers = new List<ProgressHandler>();
                }
                else
                {
                    _progressHandlers = _progressListPool.Dequeue();
                }
            }
            _progressHandlers.Add(new ProgressHandler
            {
                Callback = onProgress,
                Rejectable = rejectable
            });
        }

        // Invoke all reject handlers.
        private void InvokeRejectHandlers(Exception ex)
        {
            if (_rejectHandlers != null)
            {
                for (int i = 0; i < _rejectHandlers.Count; i++)
                {
                    var handler = _rejectHandlers[i];
                    InvokeRejectHandler(handler.Callback, handler.Rejectable, ex);
                }
            }

            ClearHandlers();
        }

        //Invoke all progress handlers.
        private void InvokeProgressHandlers(float progress)
        {
            if (_progressHandlers != null)
            {
                for (int i = 0; i < _progressHandlers.Count; i++)
                {
                    var handler = _progressHandlers[i];
                    InvokeProgressHandler(handler.Callback, handler.Rejectable, progress);
                }
            }
        }

        // Invoke all resolve handlers
        private void InvokeResolveHandlers()
        {
            if (_resolveHandlers != null)
            {
                for (int i = 0; i < _resolveHandlers.Count; i++)
                {
                    var handler = _resolveHandlers[i];
                    InvokeResolveHandler(handler.Callback, handler.Rejectable);
                }
            }

            ClearHandlers();
        }

        protected virtual void ClearHandlers()
        {
            _rejectHandlers?.Clear();
            _resolveHandlers?.Clear();
            _progressHandlers?.Clear();

            if(_resolveHandlers!=null)
            {
                _resolveListPool.Enqueue(_resolveHandlers);
            }
            if (_rejectHandlers != null)
            {
                _rejectListPool.Enqueue(_rejectHandlers);
            }
            if (_progressHandlers != null)
            {
                _progressListPool.Enqueue(_progressHandlers);
            }
        }

        private void InvokeResolveHandler(Action callback, IRejectable rejectable)
        {



            try
            {
                callback();
            }
            catch (Exception ex)
            {
                rejectable.Reject(ex);
            }
        }

        private void InvokeRejectHandler(Action<Exception> callback, IRejectable rejectable, Exception value)
        {



            try
            {
                callback(value);
            }
            catch (Exception ex)
            {
                rejectable.Reject(ex);
            }
        }

        private void InvokeProgressHandler(Action<float> callback, IRejectable rejectable, float progress)
        {

            try
            {
                callback(progress);
            }
            catch (Exception ex)
            {
                rejectable.Reject(ex);
            }
        }
        #endregion

        #region static method
        // Convert an exception directly into a rejected promise.
        public static IPromise Rejected(Exception ex)
        {
            var promise = RejectedWithoutDebug(ex);


            return promise;
        }
        public static IPromise RejectedWithoutDebug(Exception ex)
        {


            var promise = new Promise();
            promise._rejectionException = ex;
            promise.CurState = PromiseState.Rejected;

            return promise;
        }
        public static IPromise Resolved()
        {
            return ResolvedPromise;
        }

        // Raises the UnhandledException event.
        internal static void PropagateUnhandledException(object sender, Exception ex)
        {
            if (_unhandledException != null)
            {
                _unhandledException(sender, new ExceptionEventArgs(ex));
            }
        }

        /// <summary>
        ///     Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        ///     Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise Race(IEnumerable<IPromise> promises)
        {
            return ResolvedPromise.RaceInternal(promises);
        }

        /// <summary>
        ///     Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        ///     Returns the value from the first promise that has resolved.
        /// </summary>
        public static IPromise Race(params IPromise[] promises)
        {
            return ResolvedPromise.RaceInternal(promises); // Cast is required to force use of the other function.
        }

        /// <summary>
        ///     Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise All(params IPromise[] promises)
        {
            return ResolvedPromise.AllInternal((IEnumerable<IPromise>)promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        ///     Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise All(IEnumerable<IPromise> promises)
        {
            return ResolvedPromise.AllInternal(promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        ///     Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
        ///     otherwise, it will be rejected  when all of them are rejected
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise Any(params IPromise[] promises)
        {
            return ResolvedPromise.AnyInternal((IEnumerable<IPromise>)promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        ///     Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
        ///     otherwise, it will be rejected  when all of them are rejected
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        public static IPromise Any(IEnumerable<IPromise> promises)
        {
            return ResolvedPromise.AnyInternal(promises); // Cast is required to force use of the other All function.
        }
        /// <summary>
        ///     Chain a number of operations using promises.
        ///     Takes a number of functions each of which starts an async operation and yields a promise.
        /// </summary>
        public static IPromise Sequence(params Func<IPromise>[] fns)
        {
            return ResolvedPromise.SequenceInternal((IEnumerable<Func<IPromise>>)fns);
        }

        /// <summary>
        ///     Chain a sequence of operations using promises.
        ///     Takes a collection of functions each of which starts an async operation and yields a promise.
        /// </summary>
        public static IPromise Sequence(IEnumerable<Func<IPromise>> fns)
        {
            return ResolvedPromise.SequenceInternal(fns);
        }
        #endregion

        #region private extension method
        protected virtual Promise<ConvertedT> GetRawPromise<ConvertedT>()
        {
            return new Promise<ConvertedT>();
        }

        protected virtual Promise GetRawPromise()
        {
            return new Promise();
        }


        /// <summary>
        ///     Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        protected IPromise AllInternal(params IPromise[] promises)
        {
            return AllInternal((IEnumerable<IPromise>)promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        ///     Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
        ///     otherwise, it will be rejected  when all of them are rejected
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        protected IPromise AnyInternal(params IPromise[] promises)
        {
            return AnyInternal((IEnumerable<IPromise>)promises); // Cast is required to force use of the other All function.
        }

        /// <summary>
        ///     Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
        ///     otherwise, it will be rejected  when all of them are rejected
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        protected IPromise AnyInternal(IEnumerable<IPromise> promises)
        {
            var promisesArray = promises.ToArray();


            int remainingCount = promisesArray.Length;
            float[] progress = new float[remainingCount];
            var groupException = new PromiseGroupException(remainingCount);
            var resultPromise = GetRawPromise();
            resultPromise.WithName(Name);

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
                    .Then(() =>
                        {
                            progress[index] = 1f;
                            resultPromise.ReportProgress(1f);

                            if (resultPromise.CurState == PromiseState.Pending)
                            {
                                // return first fulfill promise
                                resultPromise.Resolve();
                            }
                        },
                        _ =>
                        {
                            --remainingCount;
                            if (remainingCount <= 0 && resultPromise.CurState == PromiseState.Pending)
                            {
                                // This will happen if all of the promises are rejected.
                                resultPromise.RejectWithoutDebug(groupException);
                            }
                        });
            });

            return resultPromise;
        }

        /// <summary>
        ///     Chain a number of operations using promises.
        ///     Takes a number of functions each of which starts an async operation and yields a promise.
        /// </summary>
        protected IPromise SequenceInternal(params Func<IPromise>[] fns)
        {
            return SequenceInternal((IEnumerable<Func<IPromise>>)fns);
        }

        /// <summary>
        ///     Chain a sequence of operations using promises.
        ///     Takes a collection of functions each of which starts an async operation and yields a promise.
        /// </summary>
        private IPromise SequenceInternal(IEnumerable<Func<IPromise>> fns)
        {
            var resultPromise = GetRawPromise();

            int count = 0;
            fns.Aggregate(
                    Resolved(),
                    (prevPromise, fn) =>
                    {
                        int itemSequenceCount = count++;

                        return prevPromise
                            .Then(() =>
                            {
                                float sliceLength = 1 / (float)count;
                                resultPromise.ReportProgress(itemSequenceCount * sliceLength);
                                return fn();
                            })
                            .Progress(v =>
                            {
                                float sliceLength = 1 / (float)count;
                                // v is curProgressValue 
                                // the evaluation of the formula below is
                                // ( all accumulated value + cur progress value ) * sliceRatio
                                resultPromise.ReportProgress((itemSequenceCount + v) * sliceLength);
                            });
                    }
                )
                .Then(resultPromise.Resolve, resultPromise.RejectWithoutDebug);

            return resultPromise;
        }

        /// <summary>
        ///     Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
        ///     Returns a promise of a collection of the resolved results.
        /// </summary>
        protected IPromise AllInternal(IEnumerable<IPromise> promises)
        {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0)
            {
                return Resolved();
            }
            int remainingCount = promisesArray.Length;
            var resultPromise = GetRawPromise();
            float[] progress = new float[remainingCount];
            resultPromise.WithName(Name);

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
                    .Then(() =>
                        {
                            progress[index] = 1f;

                            remainingCount--;
                            if (remainingCount == 0)
                            {
                                if (remainingCount <= 0 && resultPromise.CurState == PromiseState.Pending)
                                {
                                    // This will never happen if any of the promises has en error occurred 
                                    resultPromise.Resolve();
                                }
                            }
                            return resultPromise;
                        },
                        ex =>
                        {
                            if (resultPromise.CurState == PromiseState.Pending)
                            {
                                // If a promise has en error occurred 
                                // and the result promise is still pending, first reject it.
                                resultPromise.RejectWithoutDebug(ex);
                            }
                        });
            });

            return resultPromise;
        }
        /// <summary>
        ///     Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        ///     Returns the value from the first promise that has resolved.
        /// </summary>
        private IPromise RaceInternal(params IPromise[] promises)
        {
            return RaceInternal((IEnumerable<IPromise>)promises); // Cast is required to force use of the other function.
        }

        /// <summary>
        ///     Returns a promise that resolves when the first of the promises in the enumerable argument have resolved.
        ///     Returns the value from the first promise that has resolved.
        /// </summary>
        protected IPromise RaceInternal(IEnumerable<IPromise> promises)
        {
            var promisesArray = promises.ToArray();

            int remainingCount = promisesArray.Length;
            var resultPromise = GetRawPromise();
            float[] progress = new float[remainingCount];
            resultPromise.WithName(Name);

            promisesArray.Each((promise, index) =>
                {
                    promise
                        .Progress(v =>
                        {
                            progress[index] = v;
                            resultPromise.ReportProgress(progress.Max());
                        })
                        .Then(() =>
                            {
                                if (resultPromise.CurState == PromiseState.Pending)
                                {
                                    resultPromise.Resolve();
                                }
                            },
                            ex =>
                            {
                                if (resultPromise.CurState == PromiseState.Pending)
                                {
                                    // If a promise errored and the result promise is still pending, reject it.
                                    resultPromise.RejectWithoutDebug(ex);
                                }
                            });
                }
            );

            return resultPromise;
        }

        #endregion
    }

    /// <summary>
    ///     Represents a handler invoked when the promise is resolved.
    /// </summary>
    public struct ResolveHandler
    {
        /// <summary>
        ///     Callback fn.
        /// </summary>
        public Action Callback;

        /// <summary>
        ///     The promise that is rejected when there is an error while invoking the handler.
        /// </summary>
        public IRejectable Rejectable;
    }

    /// <summary>
    ///     Represents a handler invoked when the promise is rejected.
    /// </summary>
    public struct RejectHandler
    {
        /// <summary>
        ///     Callback fn.
        /// </summary>
        public Action<Exception> Callback;

        /// <summary>
        ///     The promise that is rejected when there is an error while invoking the handler.
        /// </summary>
        public IRejectable Rejectable;
    }

    public struct ProgressHandler
    {
        /// <summary>
        ///     Callback fn.
        /// </summary>
        public Action<float> Callback;

        /// <summary>
        ///     The promise that is rejected when there is an error while invoking the handler.
        /// </summary>
        public IRejectable Rejectable;
    }

}
