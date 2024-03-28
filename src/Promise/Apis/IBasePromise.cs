using System;
using System.Runtime.CompilerServices;
namespace RSG.Promises
{
    public interface IBasePromise : IDisposable
    {
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

    public static class PromiseStateExtension
    {
        public static RSG.PromiseTaskStatus ToTaskStatus(this PromiseState promiseState)
        {
            switch (promiseState)
            {
                case PromiseState.Pending: return RSG.PromiseTaskStatus.Pending;
                case PromiseState.Resolved: return RSG.PromiseTaskStatus.Succeeded;
                case PromiseState.Rejected:
                default:
                    return RSG.PromiseTaskStatus.Faulted;
            }
        }
    }
}
