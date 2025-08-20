using System;

namespace UnityFetch
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionAttribute : Attribute
    {
        public readonly RequestMethod method;
        public readonly string route;

        public string Name { get; set; }

        public ActionAttribute(RequestMethod method, string route)
        {
            this.method = method;
            this.route = route;
        }

        public ActionAttribute(RequestMethod method)
            : this(method, null)
        {
            this.method = method;
        }
    }
}