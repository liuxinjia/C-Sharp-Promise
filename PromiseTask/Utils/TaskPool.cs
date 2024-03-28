using System;
using System.Threading;

namespace LightAsync.Utils
{
    public interface ITaskPoolNode<T>
    {
        ref T NextNode { get; }
    }

    public struct TaskPool<T> where T : class, ITaskPoolNode<T>
    {
        internal static int MaxPoolSize = Int32.MaxValue;

        int size;
        T root;

        public int Size => size;

        public bool TryPop(out T result)
        {
            if (root != null)
            {
                var tmp = root;
                root = root.NextNode;
                tmp.NextNode = null;
                result = tmp;
                size--;
                return true;
            }

            result = default;
            return false;
        }

        public bool TryPush(T item)
        {
            if (size < MaxPoolSize)
            {
                item.NextNode = root;
                root = item;
                size++;
                return true;
            }

            return false;
        }
    }

}
