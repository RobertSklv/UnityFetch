namespace UnityFetch
{
    public class InQueryAttribute : ActionParameterAttribute
    {
        public InQueryAttribute(string alias) : base(alias)
        {
        }

        public InQueryAttribute()
        {
        }
    }
}