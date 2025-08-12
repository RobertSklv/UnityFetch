using UnityEngine.Networking;

namespace UnityFetch.RequestProcessing
{
    internal class FileRequestProcessor : RequestProcessor
    {
        public override T GenerateResponse<T>(UnityWebRequest request, UnityFetchRequestOptions options)
        {
            return default;
        }

        public override DownloadHandler GetDownloadHandler(UnityFetchRequestOptions options)
        {
            return new DownloadHandlerFile(options.DownloadedFileSavePath, options.DownloadedFileAppend);
        }
    }
}