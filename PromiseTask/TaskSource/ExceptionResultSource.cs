using System;
using System.Runtime.ExceptionServices;

namespace LightAsync
{
    public sealed class ExceptionResultSource<T> : IPromiseTaskSource<T>
    {
        readonly ExceptionDispatchInfo exception;
        bool calledGet;

        public ExceptionResultSource(Exception exception)
        {
            this.exception = ExceptionDispatchInfo.Capture(exception);
        }

        public T GetResult(short token)
        {
            if (!calledGet)
            {
                calledGet = true;
                GC.SuppressFinalize(this);
            }
            exception.Throw();
            return default;
        }

        void IPromiseTaskSource.GetResult(short token)
        {
            if (!calledGet)
            {
                calledGet = true;
                GC.SuppressFinalize(this);
            }
            exception.Throw();
        }

        public PromiseTaskStatus GetStatus(short token)
        {
            return PromiseTaskStatus.Faulted;
        }

        public PromiseTaskStatus UnsafeGetStatus()
        {
            return PromiseTaskStatus.Faulted;
        }

        public void OnCompleted(Action continuation, short token)
        {
            continuation?.Invoke();
        }

        ~ExceptionResultSource()
        {
            if (!calledGet)
            {
                //UniTaskScheduler.PublishUnobservedTaskException(exception.SourceException);
            }
        }
    }

   
}
