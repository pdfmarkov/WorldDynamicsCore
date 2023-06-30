namespace CityBuilderCore
{
    /// <summary>
    /// base class for behaviours that save data without being otherwise known to the rest of the system<br/>
    /// <see cref="DefaultGameManager"/> finds all instances and saves them with the specified key
    /// </summary>
    public abstract class ExtraDataBehaviour : KeyedBehaviour, ISaveData
    {
        public abstract void LoadData(string json);
        public abstract string SaveData();
    }
}