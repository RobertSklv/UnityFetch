namespace UnityFetch
{
    public class UnityFetchRequestException : UnityFetchException
    {
        public readonly UnityFetchResponse response;

        public UnityFetchRequestException(UnityFetchResponse response)
        {
            this.response = response;
        }

        public UnityFetchRequestException(string msg) : base(msg)
        {
        }
    }
}