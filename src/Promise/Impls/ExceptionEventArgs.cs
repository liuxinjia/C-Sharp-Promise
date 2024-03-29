using System;
namespace Cr7Sund.Promises
{
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception
        {
            get;
            private set;
        }
    }
}
