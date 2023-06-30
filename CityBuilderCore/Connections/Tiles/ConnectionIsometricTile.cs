using UnityEngine;
using UnityEngine.Tilemaps;

namespace CityBuilderCore
{
    /// <summary>
    /// special connected isometric tile that will connect to any point that has a connection<br/>
    /// this allows, for example, pipes in urban to connect to pumps and houses
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Tiles/" + nameof(ConnectionIsometricTile))]
    public class ConnectionIsometricTile : ConnectedIsometricTile
    {
        [Tooltip("the connection this tile will connect to")]
        public Connection Connection;

        protected override bool hasConnection(ITilemap tilemap, Vector3Int point)
        {
            if (base.hasConnection(tilemap, point))
                return true;

            if (Application.isPlaying)
                return Dependencies.Get<IConnectionManager>().HasPoint(Connection, (Vector2Int)point);
            else
                return false;
        }
    }
}
