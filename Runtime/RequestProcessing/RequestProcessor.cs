using UnityEngine.Networking;

namespace UnityFetch.RequestProcessing
{
    internal abstract class RequestProcessor
    {
        public abstract DownloadHandler GetDownloadHandler(UnityFetchRequestOptions options);

        public abstract T GenerateResponse<T>(UnityWebRequest request, UnityFetchRequestOptions options);

        public virtual string GetRawResponse(UnityWebRequest request)
        {
            return null;
        }
    }
}