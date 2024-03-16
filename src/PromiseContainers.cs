using System;
using System.Collections.Generic;

namespace RSG.Promises
{
    public class PromiseContainer
    {
        public Dictionary<int, Exception> _exContainers = new Dictionary<int, Exception>(16);
        public Dictionary<int, PromiseState> _stateContainers = new Dictionary<int, PromiseState>(16);

        public void ChangeEx(int id, Exception e) =>
            _exContainers[id] = e;

        public bool TryGetEx(int id, out Exception e) =>
            _exContainers.TryGetValue(id, out e);

        public void ChangeState(int id, PromiseState
            state) => _stateContainers[id] = state;
        public bool TryGetState(int id, out PromiseState state) => _stateContainers.TryGetValue(id, out state);

        public void Clear()
        {
            _exContainers.Clear();
            _stateContainers.Clear();
        }
    }

    public class PromiseValueContainer<T>
    {
        public Dictionary<int, T> _valueContainers = new Dictionary<int, T>(4);

        public void ChangeValue(int id, T value) =>
            _valueContainers[id] = value;

        public bool TryGetValue(int id, out T value) =>
            _valueContainers.TryGetValue(id, out value);

        public void Clear()
        {
            _valueContainers.Clear();
        }
    }

    public static class PromiseContainers
    {
        public static Dictionary<string, PromiseContainer> keyValuePairs = new Dictionary<string, PromiseContainer>();
    }

    public static class PromiseValueContainers<T>
    {
        public static Dictionary<string, PromiseValueContainer<T>> valueCotainers = new Dictionary<string, PromiseValueContainer<T>>();
    }

    public static class PromiseExtension
    {
        public static void ChangeEx(this IPromiseInfo promise, int id, Exception e)
        {
            PromiseContainer promiseContainer = FindEntry(promise);
            promiseContainer.ChangeEx(id, e);
        }

        public static bool TryGetEx(this IPromiseInfo promise, int id, out Exception e)
        {
            PromiseContainer promiseContainer = FindEntry(promise);

            return promiseContainer.TryGetEx(id, out e);
        }

        public static void ChangeState(this IPromiseInfo promise, int id, PromiseState
            state)
        {
            PromiseContainer promiseContainer = FindEntry(promise);
            promiseContainer.ChangeState(id, state);
        }

        public static bool TryGetState(this IPromiseInfo promise, int id, out PromiseState state)
        {
            PromiseContainer promiseContainer = FindEntry(promise);


            return promiseContainer.TryGetState(id, out state);
        }



        public static void ChangeValue<T>(this IPromiseInfo promise, int id, T value)
        {
            PromiseValueContainer<T> promiseContainer = FindEntry<T>(promise);

            promiseContainer.ChangeValue(id, value);
        }


        public static bool TryGetValue<T>(this IPromiseInfo promise, int id, out T value)
        {
            PromiseValueContainer<T> promiseContainer = FindEntry<T>(promise);

            return promiseContainer.TryGetValue(id, out value);
        }


        public static void Clear(this IPromiseInfo promise)
        {
            
        }

        public static void ClearValue<T>(this IPromiseInfo promise)
        {
            promise.Clear();
            string name = "";
            if (promise.Name == null)
            {
                name = string.Empty;
            }
            if (PromiseValueContainers<T>.valueCotainers.TryGetValue(name, out var promiseContainer))
            {
                promiseContainer.Clear();
            }
        }

        private static PromiseContainer FindEntry(IPromiseInfo promise)
        {
            string name = "";
            if (promise.Name == null)
            {
                name = string.Empty;
            }
            if (!PromiseContainers.keyValuePairs.TryGetValue(name, out var promiseContainer))
            {
                promiseContainer = new PromiseContainer();
                PromiseContainers.keyValuePairs.Add(name, promiseContainer);
            }

            return promiseContainer;
        }

        private static PromiseValueContainer<T> FindEntry<T>(IPromiseInfo promise)
        {
            string name = "";
            if (promise.Name == null)
            {
                name = string.Empty;
            }
            if (!PromiseValueContainers<T>.valueCotainers.TryGetValue(name, out var promiseContainer))
            {
                promiseContainer = new PromiseValueContainer<T>();
                PromiseValueContainers<T>.valueCotainers.Add(name, promiseContainer);
            }

            return promiseContainer;
        }
    }

}
