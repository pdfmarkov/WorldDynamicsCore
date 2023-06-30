using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// Connections are networks of values on the Map<br/>
    /// they consist of feeders which determine the values and passers that just pass them along
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(Connection))]
    public class Connection : KeyedObject
    {
        [Tooltip("name may be used in UI")]
        public string Name;

        [Tooltip("optional layer the connection can spread its values onto")]
        public Layer Layer;
        [Tooltip("range of points outside the affector")]
        public int LayerRange;
        [Tooltip("value subtracted for every step outside the affector")]
        public int LayerFalloff;
    }
}
