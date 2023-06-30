using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// behaviour that will refresh tiles under the transform of an attached building when it is placed<br/>
    /// used in the urban demo so that tiles attach to buildings, for example power lines to houses
    /// </summary>
    public class StructureTilesRefresher : MonoBehaviour
    {
        [Tooltip("the key of the structure tiles")]
        public string Key;

        private void Start()
        {
            var tiles = Dependencies.Get<IStructureManager>().GetStructure(Key) as StructureTiles;
            if (!tiles)
                return;

            var building = GetComponent<IBuilding>();

            if (building == null)
            {
                var gridPosition = Dependencies.Get<IGridPositions>().GetGridPosition(transform.position);

                tiles.RefreshTile(gridPosition);

                foreach (var point in PositionHelper.GetAdjacent(gridPosition, Vector2Int.one, true))
                {
                    tiles.RefreshTile(point);
                }
            }
            else
            {
                foreach (var point in PositionHelper.GetBoxPositions(building.Point - Vector2Int.one, building.Point + building.Size + Vector2Int.one))
                {
                    tiles.RefreshTile(point);
                }
            }
        }
    }
}
