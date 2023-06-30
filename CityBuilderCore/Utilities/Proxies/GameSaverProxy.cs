using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// behaviour that makes save/load accessible to unity events<br/>
    /// unity events could also be pointed directly to the game manager but if that is in a different prefab that can get annoying
    /// </summary>
    public class GameSaverProxy : MonoBehaviour, IGameSaver
    {
        public bool IsSaving => Dependencies.Get<IGameSaver>().IsSaving;
        public bool IsLoading => Dependencies.Get<IGameSaver>().IsLoading;

        public void Save() => Dependencies.Get<IGameSaver>().Save();
        public void SaveNamed(string name) => Dependencies.Get<IGameSaver>().SaveNamed(name);
        public void Load() => Dependencies.Get<IGameSaver>().Load();
        public void LoadNamed(string name) => Dependencies.Get<IGameSaver>().LoadNamed(name);
    }
}
