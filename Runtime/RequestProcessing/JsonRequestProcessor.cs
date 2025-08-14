using UnityEngine.Networking;

namespace UnityFetch.RequestProcessing
{
    internal class JsonRequestProcessor : RequestProcessor
    {
        public override T GenerateResponse<T>(UnityWebRequest request, UnityFetchRequestOptions options)
        {
            return options.JsonSerializer.DeserializeObject<T>(request.downloadHandler.text, options.ActionFlags);
        }

        public override string GetRawResponse(UnityWebRequest request)
        {
            return request.downloadHandler.text;
        }

        public override DownloadHandler GetDownloadHandler(UnityFetchRequestOptions options)
        {
            return new DownloadHandlerBuffer();
        }
    }
}