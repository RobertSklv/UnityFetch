using System;

namespace UnityFetch
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionFlagAttribute : Attribute
    {
        public readonly string name;
        public readonly object value;

        public ActionFlagAttribute(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }
}