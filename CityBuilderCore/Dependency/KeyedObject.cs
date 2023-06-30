using UnityEngine;

namespace CityBuilderCore
{
    public class KeyedObject : ScriptableObject, IKeyed
    {
        [Tooltip("unique identifier among a type of objects(might be used in savegames, be careful when changing)")]
        public string Key;

        string IKeyed.Key => Key;
    }
}