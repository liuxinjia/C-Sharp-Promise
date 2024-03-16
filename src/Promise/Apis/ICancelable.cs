using System;
namespace RSG.Promises
{
    /// <summary>
    ///     Interface for a promise that can be canceled
    /// </summary>
    public interface ICancelable
    {
        /// <summary>
        ///     Cancel the promise 
        /// </summary>
        void Cancel();
    }
}
