using RSG.Utils;
using System;

namespace RSG
{
    public sealed class PromiseTaskSource<T> : IPromiseTaskSource<T>, ITaskPoolNode<PromiseTaskSource<T>>
    {
        private static TaskPool<PromiseTaskSource<T>> pool;

        public PromiseTaskStatus status;
        public T result;
        public Action registerAction;
        public PromiseTaskSource<T> nextNode;

        public ref PromiseTaskSource<T> NextNode => ref nextNode;


        internal PromiseTaskSource(T value)
        {
            result = value;
        }

        public static PromiseTaskSource<T> Create(T value)
        {
            if (!pool.TryPop(out PromiseTaskSource<T> result))
            {
                result = new PromiseTaskSource<T>(value);
            }

            result.result = value;

            return result;
        }

        public T GetResult(short token)
        {
            var returnResult =  result;
            TryReturn();

            return returnResult;
        }

        public PromiseTaskStatus GetStatus(short token)
        {
            return status;
        }

        public PromiseTaskStatus UnsafeGetStatus()
        {
            return status;
        }

        void IPromiseTaskSource.GetResult(short token)
        {
            TryReturn();
        }

        public void OnCompleted(Action continuation, short token)
        {
            registerAction = continuation;
        }

        private void TryReturn()
        {
            pool.TryPush(this);
            result = default;
            status = PromiseTaskStatus.Pending;
            registerAction = null;
        }
    }
}
