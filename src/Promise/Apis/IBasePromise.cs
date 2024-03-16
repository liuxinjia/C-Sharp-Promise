using System;
namespace RSG.Promises
{
    public interface IBasePromise : IDisposable
    {
        Action<Exception> RejectHandler { get; }
        Action<float> ProgressHandler { get; }
        PromiseState CurState { get; }
        
        /// <summary>
        ///     Complete the promise. Adds a default error handler.
        /// </summary>
        void Done();
    }
    
    public enum PromiseState
    {
        Pending, // The promise is in-flight.
        Rejected, // The promise has been rejected.
        Resolved // The promise has been resolved.
    }
}
