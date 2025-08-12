namespace UnityFetch
{
    public class InRouteAttribute : ActionParameterAttribute
    {
        public InRouteAttribute(string alias) : base(alias)
        {
        }

        public InRouteAttribute()
        {
        }
    }
}