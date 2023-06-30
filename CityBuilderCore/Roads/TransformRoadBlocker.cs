using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// blocker that simply blocks its own position in the <see cref="IRoadManager"/>
    /// blocking prevents a <see cref="Walker"/> with <see cref="PathType.RoadBlocked"/> from using a point
    /// </summary>
    public class TransformRoadBlocker : MonoBehaviour
    {
        private Vector2Int _position;

        private void Start()
        {
            _position = Dependencies.Get<IGridPositions>().GetGridPosition(transform.position);

            Dependencies.Get<IRoadManager>().Block(new Vector2Int[] { _position });
        }

        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded)
                Dependencies.Get<IRoadManager>().Unblock(new Vector2Int[] { _position });
        }
    }
}