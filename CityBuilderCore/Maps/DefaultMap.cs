using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CityBuilderCore
{
    /// <summary>
    /// tilemap based map implementation<br/>
    /// whether map points are walkable or buildable depends on the tiles on a tilemap<br/>
    /// the <see cref="BuildingRequirement.GroundOptions"/> have to be tile when used with this map
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public class DefaultMap : MapBase
    {
        [Tooltip("tilemap containing the blocking tiles, if empty nothing is blocked")]
        public Tilemap Ground;
        [Tooltip("tiles that are blocked in mapgrid pathfinding")]
        public TileBase[] WalkingBlockingTiles;
        [Tooltip("tiles that are blocked for building")]
        public BlockingTile[] BuildingBlockingTiles;

        public override bool IsBuildable(Vector2Int point, int mask)
        {
            if (Ground)
                return !BuildingBlockingTiles.Where(b => b.Level.Check(mask)).Select(b => b.Tile).Contains(Ground.GetTile((Vector3Int)point));
            else
                return true;
        }

        public override bool IsWalkable(Vector2Int point)
        {
            if (Ground)
                return !WalkingBlockingTiles.Contains(Ground.GetTile((Vector3Int)point));
            else
                return true;
        }

        public override bool CheckGround(Vector2Int point, Object[] options)
        {
            return options.Contains(Ground.GetTile((Vector3Int)point));
        }
    }
}