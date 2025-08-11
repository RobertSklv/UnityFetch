namespace UnityFetch
{
    public interface IJsonSerializer
    {
        string SerializeObject(object? value);

        T? DeserializeObject<T>(string value);
    }
}