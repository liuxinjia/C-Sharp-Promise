using System;
using System.Collections.Generic;

namespace RSG
{
    public static class PromiseValueContainers
    {
        public static Dictionary<int, Exception> _exCotainers = new Dictionary<int, Exception>();
        public static Dictionary<int, PromiseState> _stateContainers = new Dictionary<int, PromiseState>();

        public static void CreateEx(int id, Exception e)
        {
            _exCotainers[id] = e;
        }

        public static bool TryGetEx(int id, out Exception e)
        {
            return _exCotainers.TryGetValue(id, out e);
        }

        public static void ChangeState(int id, PromiseState
            state) => _stateContainers[id] = state;
        public static bool TryGetState(int id, out PromiseState state) => _stateContainers.TryGetValue(id, out state);
    }
}
