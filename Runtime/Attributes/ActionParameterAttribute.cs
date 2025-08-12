using System;

namespace UnityFetch
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class ActionParameterAttribute : Attribute
    {
        public readonly string alias;

        public ActionParameterAttribute(string alias)
        {
            this.alias = alias;
        }

        public ActionParameterAttribute()
        {
        }
    }
}