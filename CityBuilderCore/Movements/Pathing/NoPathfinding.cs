using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// fallback pathfinding that just returns a straight path from start to target
    /// </summary>
    public class NoPathfinding : IPathfinder
    {
        public WalkingPath FindPath(Vector2Int[] starts, Vector2Int[] targets, object parameter = null)
        {
            if (starts.Length == 0 || targets.Length == 0)
                return null;

            return new WalkingPath(new[] { starts[0], targets[0] });
        }

        public bool HasPoint(Vector2Int point, object tag = null)
        {
            return true;
        }
    }
}