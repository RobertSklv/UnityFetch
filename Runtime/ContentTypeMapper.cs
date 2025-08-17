using System.Collections.Generic;
using System;

namespace UnityFetch
{
    public static class ContentTypeMapper
    {
        private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
        {
            // Documents
            { "text/html", "document" },
            { "application/xhtml+xml", "document" },
        
            // Scripts
            { "application/javascript", "script" },
            { "text/javascript", "script" },
            { "application/x-javascript", "script" },
            { "application/json", "json" },
        
            // Stylesheets
            { "text/css", "stylesheet" },
        
            // Images
            { "image/png", "texture" },
            { "image/jpeg", "texture" },
            { "image/gif", "texture" },
            { "image/webp", "texture" },
            { "image/svg+xml", "texture" },
        
            // Fonts
            { "font/woff", "font" },
            { "font/woff2", "font" },
            { "application/font-woff", "font" },
            { "application/x-font-ttf", "font" },
        
            // Media
            { "audio/mpeg", "media" },
            { "audio/ogg", "media" },
            { "audio/wav", "media" },
            { "video/mp4", "media" },
            { "video/webm", "media" },
        
            // Others
            { "text/plain", "text" },
            { "application/xml", "xml" },
            { "application/octet-stream", "binary" }
        };

        public static string GetFriendlyContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return "other";
            }

            // Strip any parameters (e.g., "text/html; charset=utf-8")
            string mime = contentType.Split(';')[0].Trim();

            if (Map.TryGetValue(mime, out var friendly))
            {
                return friendly;
            }

            // Handle broad categories
            if (mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "texture";
            }

            if (mime.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return "media";
            }

            if (mime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                return "media";
            }

            if (mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            {
                return "text";
            }

            return "other";
        }
    }
}