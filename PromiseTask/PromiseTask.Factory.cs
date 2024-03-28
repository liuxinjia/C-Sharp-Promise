using System;
using System.Threading.Tasks;

namespace LightAsync
{

    public partial struct PromiseTask<T>
    {
        //private static readonly PromiseTask CanceledUniTask = new Func<UniTask>(() =>
        //{
        //    return new UniTask(new CanceledResultSource(CancellationToken.None), 0);
        //})();

        public static PromiseTask<T> FromException(Exception ex)
        {
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

        public static PromiseTask<T> FromPromise(T value)
        {
            var source = PromiseTaskSource<T>.Create(value);
            source.status = PromiseTaskStatus.Succeeded; // for performance test
            return new PromiseTask<T>(source, 0);
        }


    }
}
