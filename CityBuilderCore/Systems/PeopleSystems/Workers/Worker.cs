using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// a type of worker(mason, carpenter, laborer)
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(Worker))]
    public class Worker : ScriptableObject
    {
        [Tooltip("name of the worker for use in UI")]
        public string Name;
        [Tooltip("icon of the worker for use in UI")]
        public Sprite Icon;
    }
}