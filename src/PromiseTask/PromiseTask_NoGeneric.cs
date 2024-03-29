using System.Runtime.CompilerServices;
using RSG.CompilerServices;

namespace RSG
{
    [AsyncMethodBuilder(typeof(PromiseTaskMethodBuilder))]
    public partial struct PromiseTask
    {
        public static readonly PromiseTask CompletedTask = new PromiseTask();

        internal readonly IPromiseTaskSource source;
        internal readonly short token;

        public PromiseTaskStatus Status
        {
            get
            {
                return (source == null) ? PromiseTaskStatus.Succeeded : source.GetStatus(token);
            }
        }

        public PromiseTask(short token = 10)
        {
            this.source = null;
            this.token = token;
        }

        public PromiseTask(IPromiseTaskSource source, short token)
        {
            this.source = source;
            this.token = token;
        }

        public PromiseTaskAwaiter GetAwaiter() => new PromiseTaskAwaiter(this);

    }

}