using System;

namespace UnityFetch
{
    public struct DynamicValue
    {
        private readonly Func<string> callback;

        public DynamicValue(Func<string> callback)
        {
            this.callback = callback;
        }

        public override readonly string ToString()
        {
            return callback();
        }
    }
}