using System;
using System.Runtime.CompilerServices;

namespace Cr7Sund.Promise.Utils
{
    public interface IPoolNode<T>
    {
        ref T NextNode { get; }
        bool IsRecycled { get; set; }
    }

    public struct ReusablePool<T> where T : class, IPoolNode<T>
    {
        internal static int MaxPoolSize = int.MaxValue;

        int size;
        T root;

        public int Size => size;



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var v = root;
            if (!(v is null))
            {
                ref var nextNode = ref v.NextNode;
                root = nextNode;
                nextNode = null;
                size--;
                result = v;
                result.IsRecycled = false;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(T item)
        {
            if (item.IsRecycled)
            {
                throw new Exception("item is alread recycled");
            }

            if (size < MaxPoolSize)
            {
                item.IsRecycled = true;
                item.NextNode = root;
                root = item;
                size++;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
