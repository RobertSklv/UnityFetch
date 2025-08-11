using System.Collections.Generic;

namespace UnityFetch
{
    public class ApiErrorResponse
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public string? Detail { get; set; }

        public long Status { get; set; }

        public string TraceId { get; set; }

        public Dictionary<string, string> Errors { get; set; }
    }
}