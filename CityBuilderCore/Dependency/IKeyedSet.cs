namespace CityBuilderCore
{
    public interface IKeyedSet<T> : IObjectSet<T>
    {
        T GetObject(string key);
    }
}