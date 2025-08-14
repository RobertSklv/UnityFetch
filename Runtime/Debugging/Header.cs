using System;

namespace UnityFetch.Debugging
{
    [Serializable]
    public class Header
    {
        public string name;

        public string value;

        public Header(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}