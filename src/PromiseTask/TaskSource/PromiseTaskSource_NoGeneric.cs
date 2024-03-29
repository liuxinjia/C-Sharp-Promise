using RSG.Utils;
using System;

namespace RSG
{
    public sealed class PromiseTaskSource : IPromiseTaskSource, ITaskPoolNode<PromiseTaskSource>
    {
        private static TaskPool<PromiseTaskSource> pool;

        public PromiseTaskStatus status;
        public Action registerAction;
        public PromiseTaskSource nextNode;

        public ref PromiseTaskSource NextNode => ref nextNode;



        public static PromiseTaskSource Create()
        {
            if (!pool.TryPop(out PromiseTaskSource result))
            {
                result = new PromiseTaskSource();
            }

            return result;
        }

        public void GetResult(short token)
        {
            TryReturn();
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
            status = PromiseTaskStatus.Pending;
            registerAction = null;
        }
    }
}
