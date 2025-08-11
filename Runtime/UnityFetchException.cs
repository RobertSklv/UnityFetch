using System;

namespace UnityFetch
{
    public class UnityFetchException : Exception
    {
        public UnityFetchException() { }

        public UnityFetchException(string msg) : base(msg) { }
    }
}