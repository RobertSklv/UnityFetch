using UnityEngine.Networking;

namespace UnityFetch.RequestProcessing
{
    internal class TextureRequestProcessor : RequestProcessor
    {
        public override T GenerateResponse<T>(UnityWebRequest request, UnityFetchRequestOptions options)
        {
            return (T)(object)DownloadHandlerTexture.GetContent(request);
        }

        public override DownloadHandler GetDownloadHandler(UnityFetchRequestOptions options)
        {
            return new DownloadHandlerTexture(options.DownloadedTextureParams);
        }
    }
}