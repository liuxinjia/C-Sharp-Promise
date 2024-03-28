using System;
using System.Collections.Generic;
using System.Linq;

namespace RSG
{
    public static class PromiseTaskExtension
    {
        public static PromiseTask<T> ToPromiseTask<T>(this T a)
        {
            return PromiseTask<T>.FromResult(a);
        }

        public static PromiseTask<T> ToPromiseTask<T>(this Exception a)
        {
            return PromiseTask<T>.FromException(a);
        }
    }
    public partial struct PromiseTask<T>
    {
        //private static readonly PromiseTask CanceledUniTask = new Func<UniTask>(() =>
        //{
        //    return new UniTask(new CanceledResultSource(CancellationToken.None), 0);
        //})();

        public static PromiseTask<T> FromException(Exception ex)
        {
            // PLAN: Handle Cancele
            //if (ex is OperationCanceledException oce)
            //{
            //    return FromCanceled<T>(oce.CancellationToken);
            //}

            return new PromiseTask<T>(new ExceptionResultSource<T>(ex), 0);
        }

        public static PromiseTask<T> FromResult(T value)
        {
            return new PromiseTask<T>(value);
        }

        public static PromiseTask<T[]> WhenAll(params PromiseTask<T>[] tasks)
        {
            return WhenAll((IEnumerable<PromiseTask<T>>)tasks.ToArray());
        }
        public static PromiseTask<T[]> WhenAll(IEnumerable<PromiseTask<T>> tasks)
        {
            PromiseTask<T>[] promiseTasks = tasks.ToArray();
            var promise = new WhenAllPromiseTaskSource<T>(promiseTasks, promiseTasks.Length); // consumed array in constructor.
            return new PromiseTask<T[]>(promise, 0);
        }
    }

}
