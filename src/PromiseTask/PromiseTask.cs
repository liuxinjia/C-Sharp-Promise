using System.Runtime.CompilerServices;
using Cr7Sund.CompilerServices;

namespace Cr7Sund
{


    [AsyncMethodBuilder(typeof(PromiseTaskMethodBuilder<>))]
    public partial struct PromiseTask<T>
    {
        internal readonly IPromiseTaskSource<T> source;
        internal readonly T result;
        internal readonly short token;

        public PromiseTaskStatus Status
        {
            get
            {
                return (source == null) ? PromiseTaskStatus.Succeeded : source.GetStatus(token);
            }
        }

        public PromiseTask(T result, short token = 10)
        {
            this.source = null;
            this.token = token;
            this.result = result;
        }

        public PromiseTask(IPromiseTaskSource<T> source, short token)
        {
            this.source = source;
            this.token = token;
            this.result = default;
        }

        public PromiseTaskAwaiter<T> GetAwaiter() => new PromiseTaskAwaiter<T>(this);

    }

}