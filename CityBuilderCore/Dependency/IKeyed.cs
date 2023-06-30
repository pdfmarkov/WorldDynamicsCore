namespace CityBuilderCore
{
    public interface IKeyed
    {
        /// <summary>
        /// unique identifier among a type of objects(might be used in savegames, be careful when changing)
        /// </summary>
        string Key { get; }
    }
}