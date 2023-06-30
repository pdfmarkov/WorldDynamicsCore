using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// terrain based map implementation, whether map points are buildable depends on the terrain<br/>
    /// currently only checks if the height of the terrain is in an acceptable range to see if it can be built on
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public class TerrainMap : MapBase
    {
        [Tooltip("terrain that will be sampled when height is checked")]
        public Terrain Terrain;
        [Tooltip("inclusive minimum height that is acceptable for building")]
        public float MinHeight;
        [Tooltip("inclusive maximum height that is acctptable for building")]
        public float MaxHeight;

        public override bool CheckGround(Vector2Int position, Object[] options)
        {
            return true;
        }

        public override bool IsBuildable(Vector2Int position, int mask)
        {
            var height = Terrain.SampleHeight(GetCenterFromPosition(GetWorldPosition(position)));
            
            return height >= MinHeight && height <= MaxHeight;
        }

        public override bool IsWalkable(Vector2Int position)
        {
            return true;
        }
    }
}
