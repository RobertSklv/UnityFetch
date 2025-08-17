using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine.Networking;
using System.Linq;

namespace UnityFetch
{
    public static class Util
    {
        public static string UriCombine(params string[] urns)
        {
            List<string> tokens = new();

            for (int i = 0; i < urns.Length; i++)
            {
                if (string.IsNullOrEmpty(urns[i])) continue;

                tokens.Add(urns[i].Trim('/'));
            }

            return string.Join('/', tokens).Trim('/');
        }

        public static string UriCombine(IEnumerable<string> path)
        {
            return UriCombine(path.ToArray());
        }

        public static Dictionary<string, string> ConvertParamDictionary(Dictionary<string, object> parameters)
        {
            Dictionary<string, string> converted = new();

            foreach ((string key, object value) in parameters)
            {
                converted.Add(key, value.ToString());
            }

            return converted;
        }

        public static Dictionary<string, object> GetAnonymousObjectParameters(object parameters)
        {
            Dictionary<string, object> converted = new();

            PropertyInfo[] properties = parameters.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                object value = property.GetValue(parameters, null);

                converted.Add(name, value);
            }

            return converted;
        }

        public static string EncodeUrlBody(IDictionary<string, object> queryParameters)
        {
            if (queryParameters.Count == 0)
            {
                return string.Empty;
            }

            List<string> keyValuePairs = new();

            foreach ((string key, object value) in queryParameters)
            {
                foreach (string kvp in BuildQueryParamKeyValuePair(key, value))
                {
                    keyValuePairs.Add(kvp);
                }
            }

            return string.Join('&', keyValuePairs);
        }

        public static string EncodeUrlBody(IDictionary<string, string> queryParameters)
        {
            if (queryParameters.Count == 0)
            {
                return string.Empty;
            }

            List<string> keyValuePairs = new();

            foreach ((string key, string value) in queryParameters)
            {
                foreach (string kvp in BuildQueryParamKeyValuePair(key, value))
                {
                    keyValuePairs.Add(kvp);
                }
            }

            return string.Join('&', keyValuePairs);
        }

        public static IEnumerable<string> BuildQueryParamKeyValuePair(string key, object value)
        {
            string encodedKey = UnityWebRequest.EscapeURL(key);
            string encodedValue = null;

            if (value != null)
            {
                Type valueType = value.GetType();

                if (valueType.IsValueType)
                {
                    encodedValue = UnityWebRequest.EscapeURL(value.ToString());
                }
                else if (value is string str)
                {
                    encodedValue = UnityWebRequest.EscapeURL(str);
                }
                else if (valueType.IsArray)
                {
                    Array arr = (Array)value;

                    foreach (object element in arr)
                    {
                        if (element != null && !element.GetType().IsValueType)
                        {
                            throw new UnityFetchException("Only value types are supported as query string array elements.");
                        }

                        foreach (string kvp in BuildQueryParamKeyValuePair(key, element))
                        {
                            yield return kvp;
                        }
                    }

                    yield break;
                }
                else if (valueType.IsClass && !valueType.IsAbstract)
                {
                    PropertyInfo[] properties = valueType.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        string name = property.Name;
                        object propValue = property.GetValue(value, null);

                        foreach (string kvp in BuildQueryParamKeyValuePair(encodedKey + '.' + name, propValue))
                        {
                            yield return kvp;
                        }
                    }

                    yield break;
                }
            }

            yield return encodedKey + "=" + encodedValue;
        }

        public static string FormatBytes(ulong bytes)
        {
            string[] suffixes = { "kB", "MB", "GB", "TB", "PB", "EB" };
            int i = 0;
            double dblBytes = bytes / 1024;

            while (dblBytes >= 1024 && i < suffixes.Length - 1)
            {
                dblBytes /= 1024;
                i++;
            }

            return $"{dblBytes:0.0} {suffixes[i]}";
        }

        internal static string PluralizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return word;

            word = word.ToLower();

            if (word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") ||
                word.EndsWith("ch") || word.EndsWith("sh"))
            {
                return word + "es";
            }

            if (word.EndsWith("y") && word.Length > 1 && IsConsonant(word[^2]))
            {
                return word[..^1] + "ies";
            }

            if (word.EndsWith("f"))
            {
                return word[..^1] + "ves";
            }

            if (word.EndsWith("fe"))
            {
                return word[..^2] + "ves";
            }

            return word + "s";
        }

        internal static string SingularizeWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return word;

            word = word.ToLower();

            if (word.EndsWith("ies") && word.Length > 3)
            {
                return word[..^3] + "y";
            }

            if (word.EndsWith("ves") && word.Length > 3)
            {
                return word[..^3] + "f";
            }

            if (word.EndsWith("es") && word.Length > 2)
            {
                string stem = word[..^2];
                if (stem.EndsWith("s") || stem.EndsWith("x") || stem.EndsWith("z") ||
                    stem.EndsWith("ch") || stem.EndsWith("sh"))
                {
                    return stem;
                }
            }

            if (word.EndsWith("s") && word.Length > 1)
            {
                return word[..^1];
            }

            return word;
        }

        private static bool IsConsonant(char c)
        {
            return "aeiou".IndexOf(c) == -1;
        }

        internal static string CapitalizeFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return char.ToUpper(s[0]) + s[1..];
        }

        public static ResponseStatus StatusCodeToResponseStatus(long statusCode)
        {
            return (ResponseStatus)statusCode;
        }

        public static string StatusCodeAsLabel(ResponseStatus status)
        {
            switch (status)
            {
                case ResponseStatus.Continue: return "Continue";
                case ResponseStatus.SwitchingProtocols: return "Switching Protocols";
                case ResponseStatus.Processing: return "Processing";
                case ResponseStatus.EarlyHints: return "Early Hints";

                case ResponseStatus.OK: return "OK";
                case ResponseStatus.Created: return "Created";
                case ResponseStatus.Accepted: return "Accepted";
                case ResponseStatus.NonAuthoritativeInformation: return "Non-Authoritative Information";
                case ResponseStatus.NoContent: return "No Content";
                case ResponseStatus.ResetContent: return "Reset Content";
                case ResponseStatus.PartialContent: return "Partial Content";
                case ResponseStatus.IMUsed: return "IM Used";

                case ResponseStatus.MultipleChoices: return "Multiple Choices";
                case ResponseStatus.MovedPermanently: return "Moved Permanently";
                case ResponseStatus.Found: return "Found";
                case ResponseStatus.SeeOther: return "See Other";
                case ResponseStatus.NotModified: return "Not Modified";
                case ResponseStatus.TemporaryRedirect: return "Temporary Redirect";
                case ResponseStatus.PermanentRedirect: return "Permanent Redirect";

                case ResponseStatus.BadRequest: return "Bad Request";
                case ResponseStatus.Unauthorized: return "Unauthorized";
                case ResponseStatus.PaymentRequired: return "Payment Required";
                case ResponseStatus.Forbidden: return "Forbidden";
                case ResponseStatus.NotFound: return "Not Found";
                case ResponseStatus.MethodNotAllowed: return "Method Not Allowed";
                case ResponseStatus.NotAcceptable: return "Not Acceptable";
                case ResponseStatus.ProxyAuthenticationRequired: return "Proxy Authentication Required";
                case ResponseStatus.RequestTimeout: return "Request Timeout";
                case ResponseStatus.Conflict: return "Conflict";
                case ResponseStatus.Gone: return "Gone";
                case ResponseStatus.LengthRequired: return "Length Required";
                case ResponseStatus.PreconditionFailed: return "Precondition Failed";
                case ResponseStatus.ContentTooLarge: return "Content Too Large";
                case ResponseStatus.URITooLong: return "URI Too Long";
                case ResponseStatus.UnsupportedMediaType: return "Unsupported Media Type";
                case ResponseStatus.RangeNotSatisfiable: return "Range Not Satisfiable";
                case ResponseStatus.ExpectationFailed: return "Expectation Failed";
                case ResponseStatus.MisdirectedRequest: return "Misdirected Request";
                case ResponseStatus.TooEarly: return "Too Early";
                case ResponseStatus.UpgradeRequired: return "Upgrade Required";
                case ResponseStatus.PreconditionRequired: return "Precondition Required";
                case ResponseStatus.TooManyRequests: return "Too Many Requests";
                case ResponseStatus.RequestHeaderFieldsTooLarge: return "Request Header Fields Too Large";
                case ResponseStatus.UnavailableForLegalReasons: return "Unavailable For Legal Reasons";

                case ResponseStatus.InternalServerError: return "Internal Server Error";
                case ResponseStatus.NotImplemented: return "Not Implemented";
                case ResponseStatus.BadGateway: return "Bad Gateway";
                case ResponseStatus.ServiceUnavailable: return "Service Unavailable";
                case ResponseStatus.GatewayTimeout: return "Gateway Timeout";
                case ResponseStatus.HTTPVersionNotSupported: return "HTTP Version Not Supported";
                case ResponseStatus.VariantAlsoNegotiates: return "Variant Also Negotiates";
                case ResponseStatus.NotExtended: return "Not Extended";
                case ResponseStatus.NetworkAuthenticationRequired: return "Network Authentication Required";

                default: return string.Empty;
            }
        }
    }
}