using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// sets the heights of entities depending on whether an entity is on a road or the map<br/>
    /// a terrain can be defined that will be sampled and added for entities on the map
    /// </summary>
    public class DefaultMapHeight : MonoBehaviour, IGridHeights
    {
        [Tooltip("optional terrain that will sampled and added to heights")]
        public Terrain Terrain;
        [Tooltip("height for entities on roads")]
        public float RoadHeight;
        [Tooltip("height for entities not on roads, can be modified by terrain")]
        public float MapHeight;

        private LazyDependency<IMap> _map = new LazyDependency<IMap>();

        private void Awake()
        {
            Dependencies.Register<IGridHeights>(this);
        }

        public float GetHeight(Vector3 position, PathType pathType = PathType.Map)
        {
            float height;

            switch (pathType)
            {
                case PathType.Road:
                case PathType.RoadBlocked:
                    height = RoadHeight;
                    break;
                default:
                    height = MapHeight;
                    break;
            }

            if (Terrain)
                height += Terrain.SampleHeight(position);

            return height;
        }

        public void ApplyHeight(Transform transform, Vector3 position, PathType pathType = PathType.Map, float? overrideValue = null)
        {
            var height = overrideValue.HasValue ? overrideValue.Value : GetHeight(position, pathType);

            if (_map.Value.IsXY)
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, height);
            else
                transform.localPosition = new Vector3(transform.localPosition.x, height, transform.localPosition.z);
        }
    }
}
