using System;
using System.Collections.Generic;

namespace UnityFetch
{
    public class AbortSignal
    {
        private readonly List<Action> callbacks = new();

        public void AddListener(Action callback)
        {
            callbacks.Add(callback);
        }

        public void Abort()
        {
            callbacks.ForEach(c => c?.Invoke());
        }
    }
}