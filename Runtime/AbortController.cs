namespace UnityFetch
{
    public class AbortController
    {
        public bool IsAborted { get; private set; }

        internal AbortSignal AbortSignal { get; private set; } = new();

        public void Abort()
        {
            if (IsAborted) return;

            IsAborted = true;
            AbortSignal.Abort();
        }
    }
}