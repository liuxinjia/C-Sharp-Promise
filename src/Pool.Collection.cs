using System.Collections.Generic;

namespace RSG.Promise
{
    public class QueuePoolNode<T> : Queue<T>, IPoolNode<QueuePoolNode<T>>
    {
        private QueuePoolNode<T> _poolListNode;

        public ref QueuePoolNode<T> NextNode => ref _poolListNode;
    }

    public class ListPoolNode<T> : List<T>, IPoolNode<ListPoolNode<T>>
    {
        private ListPoolNode<T> _poolListNode;

        public ref ListPoolNode<T> NextNode => ref _poolListNode;
    }

}
