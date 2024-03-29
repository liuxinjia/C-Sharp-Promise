using System;
using System.Runtime.CompilerServices;
namespace Cr7Sund.Promises
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
        public static Cr7Sund.PromiseTaskStatus ToTaskStatus(this PromiseState promiseState)
        {
            switch (promiseState)
            {
                case PromiseState.Pending: return Cr7Sund.PromiseTaskStatus.Pending;
                case PromiseState.Resolved: return Cr7Sund.PromiseTaskStatus.Succeeded;
                case PromiseState.Rejected:
                default:
                    return Cr7Sund.PromiseTaskStatus.Faulted;
            }
        }
    }
}
