using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal static class RequestStrategyFactory
    {
        public static Dictionary<string, string> MimeTypeLookupTable = new()
        {
            { ".png", "image/png" },
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".jfif", "image/jpeg" },
            { ".pjpeg", "image/jpeg" },
            { ".pjp", "image/jpeg" },
            { ".avif", "image/avif" },
            { ".gif", "image/gif" },
            { ".tif", "image/tiff" },
            { ".svg", "image/svg+xml" },
            { ".obj", "model/obj" },
            { ".txt", "text/plain" },
            { ".csv", "text/csv" },
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".oga", "audio/ogg" },
            { ".mp4", "video/mp4" },
            { ".ogv", "video/ogg" },
            { ".json", "application/json" },
            { ".pdf", "application/pdf" },
            { ".zip", "application/zip" },
            { ".xml", "application/xml" },
        };

        public static RequestStrategy Create(object data, string? fileName, UnityFetchRequestOptions options)
        {
            options.Headers.TryGetValue("Content-Type", out object contentTypeObj);
            string contentType = contentTypeObj as string;

            contentType ??= InferPayloadContentType(data, fileName);
            options.SetContentType(contentType);

            if (contentType == "application/json")
            {
                return new ApplicationJsonStrategy();
            }
            else if (contentType == "multipart/form-data")
            {
                return new MultiparyFormDataStrategy();
            }
            else if (contentType.StartsWith("image/"))
            {
                return new FileUploadStrategy();
            }
            else if (contentType == "application/x-www-form-urlencoded")
            {
                return new ApplicationXWwwFormUrlencoded();
            }
            else if (contentType == "text/plain")
            {
                return new TextPlainStrategy();
            }
            else if (contentType == "application/graphql")
            {
                return new ApplicationGraphQlStrategy();
            }
            else if (contentType == "application/xml")
            {
                return new ApplicationXmlStrategy();
            }
            else if (contentType == "application/x-www-form-urlencoded")
            {
                return new ApplicationXmlStrategy();
            }

            return new FileUploadStrategy();
        }

        public static string InferPayloadContentType(object data, string? fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string ext = Path.GetExtension(fileName).ToLower();

                if (MimeTypeLookupTable.TryGetValue(ext, out var mime)) return mime;
            }

            if (data is string s)
            {
                if (s.TrimStart().StartsWith("{") || s.TrimStart().StartsWith("[")) return "application/json";

                return "text/plain";
            }

            if (data is WWWForm || data is List<IMultipartFormSection>)
            {
                return "multipart/form-data";
            }

            if (data is byte[] bytes)
            {
                if (bytes.Length > 4 && bytes[0] == 0x89 && bytes[1] == 0x50) return "image/png";
                if (bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8) return "image/jpeg";
                if (bytes.Length > 4 && Encoding.ASCII.GetString(bytes, 0, 4) == "%PDF") return "application/pdf";

                return "application/octet-stream";
            }

            return "application/octet-stream";
        }
    }
}