using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// behaviour that expands a box collider in accordance to an <see cref="ExpandableBuilding"/>s size<br/>
    /// the bridge in THREE uses this for the collider that overrides walker heights(Bridge>Pivot>Height)
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ExpandableCollider : MonoBehaviour
    {
        public ExpandableBuilding ExpandableBuilding;
        public Vector3 BaseSize;
        public Vector3 MultipliedSize;

        private BoxCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
        }

        private void Start()
        {
            ExpandableBuilding.ExpansionChanged += updateSize;
            updateSize(ExpandableBuilding.Expansion);
        }

        private void updateSize(Vector2Int expansion)
        {
            _collider.size = BaseSize + (Dependencies.Get<IMap>().IsXY ? new Vector3(expansion.x * MultipliedSize.x, expansion.y * MultipliedSize.y, 0) : new Vector3(expansion.x * MultipliedSize.x, 0, expansion.y * MultipliedSize.y));
        }
    }
}
