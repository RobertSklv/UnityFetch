namespace UnityFetch.RequestProcessing
{
    internal static class RequestProcessorFactory
    {
        public static RequestProcessor Create(RequestMethod method, DownloadHandlerType downloadHandlerType)
        {
            if (method == RequestMethod.GET && downloadHandlerType == DownloadHandlerType.Texture)
            {
                return new TextureRequestProcessor();
            }
            else if (method == RequestMethod.GET && downloadHandlerType == DownloadHandlerType.File)
            {
                return new FileRequestProcessor();
            }

            return new JsonRequestProcessor();
        }
    }
}