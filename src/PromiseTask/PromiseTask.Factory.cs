using System;
using System.Collections.Generic;
using System.Linq;

namespace RSG
{
    public partial struct PromiseTask<T>
    {
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

    public partial struct PromiseTask
    {
        public static PromiseTask FromException(Exception ex)
        {
            return new PromiseTask(new ExceptionResultSource(ex), 0);
        }

        public static PromiseTask WhenAll(params PromiseTask[] tasks)
        {
            return WhenAll((IEnumerable<PromiseTask>)tasks.ToArray());
        }


        public static PromiseTask WhenAll(IEnumerable<PromiseTask> tasks)
        {
            PromiseTask[] promiseTasks = tasks.ToArray();
            var promise = new WhenAllPromiseTaskSource(promiseTasks, promiseTasks.Length); // consumed array in constructor.
            return new PromiseTask(promise, 0);
        }
    }
}
