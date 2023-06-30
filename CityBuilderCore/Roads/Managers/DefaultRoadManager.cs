using UnityEngine;
using UnityEngine.Tilemaps;

namespace CityBuilderCore
{
    /// <summary>
    /// simple road manager implementation that creates a single road network out of any <see cref="Road"/> added<br/>
    /// this means walkers will be able to use any road, if you need seperate road networks per road use <see cref="MultiRoadManager"/><br/>
    /// roads are visualized on the <see cref="Tilemap"/> on the same gameobject as the manager
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class DefaultRoadManager : RoadManagerBase
    {
        protected override RoadNetwork createNetwork()
        {
            return new TilemapRoadNetwork(null, GetComponent<Tilemap>(), Level.Value);
        }
    }
}