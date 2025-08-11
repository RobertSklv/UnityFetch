using System;
using UnityEngine.Networking;

namespace UnityFetch
{
    public class UnityFetchTransportException : Exception
    {
        public readonly UnityWebRequest.Result result;

        public UnityFetchTransportException(UnityWebRequest.Result result, string msg)
            : base(msg)
        {
            this.result = result;
        }

        public UnityFetchTransportException(UnityWebRequest.Result result)
            : this(result, UnityWebRequestResultToMessage(result))
        {
        }

        private static string UnityWebRequestResultToMessage(UnityWebRequest.Result result)
        {
            switch (result)
            {
                case UnityWebRequest.Result.Success: return "The request succeeded.";
                case UnityWebRequest.Result.ConnectionError: return "Failed to communicate with the server. For example, the request couldn't connect or it could not establish a secure channel.";
                case UnityWebRequest.Result.InProgress: return "The request hasn't finished yet.";
                case UnityWebRequest.Result.ProtocolError: return "The server returned an error response. The request succeeded in communicating with the server, but received an error as defined by the connection protocol.";
                case UnityWebRequest.Result.DataProcessingError: return "Error processing data. The request succeeded in communicating with the server, but encountered an error when processing the received data. For example, the data was corrupted or not in the correct format.";
                default: return result.ToString();
            }
        }
    }
}