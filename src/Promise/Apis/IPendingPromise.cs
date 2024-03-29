namespace Cr7Sund.Promises
{
    /// <summary>
    ///     Interface for a promise that can be rejected or resolved.
    /// </summary>
    public interface IPendingPromise<PromisedT> : IPromiseInfo, IRejectable, ICancelable
    {
        /// <summary>
        ///     Resolve the promise with a particular value.
        /// </summary>
        void Resolve(PromisedT value);

        /// <summary>
        ///     Report progress in a promise.
        /// </summary>
        void ReportProgress(float progress);
    }

    /// <summary>
    ///     Interface for a promise that can be rejected or resolved.
    /// </summary>
    public interface IPendingPromise : IPromiseInfo, IRejectable, ICancelable
    {
        /// <summary>
        ///     Resolve the promise.
        /// </summary>
        void Resolve();

        /// <summary>
        ///     Report progress in a promise.
        /// </summary>
        void ReportProgress(float progress);
    }
}
