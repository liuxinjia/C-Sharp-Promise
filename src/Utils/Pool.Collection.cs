using System.Collections.Generic;

namespace Cr7Sund.Promise.Utils
{
    public class QueuePoolNode<T> : Queue<T>, IPoolNode<QueuePoolNode<T>>
    {
        private QueuePoolNode<T> _poolListNode;


        public ref QueuePoolNode<T> NextNode => ref _poolListNode;
        public bool IsRecycled { get; set; }
    }

    public class ListPoolNode<T> : List<T>, IPoolNode<ListPoolNode<T>>
    {
        private ListPoolNode<T> _poolListNode;

        public ref ListPoolNode<T> NextNode => ref _poolListNode;
        public bool IsRecycled { get; set; }
    }

}
