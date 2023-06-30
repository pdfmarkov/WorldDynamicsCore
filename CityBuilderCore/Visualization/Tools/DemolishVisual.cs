using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// moves the visuals of a building over to itself before the building really gets destroyed<br/>
    /// therefore the building destruction can be animated while the building has actually already been destroyed
    /// </summary>
    public class DemolishVisual : MonoBehaviour
    {
        [Tooltip("building visuals will be moved from the building pivot to this one before it is destroyed")]
        public Transform Pivot;

        public void Remove()
        {
            Destroy(gameObject);
        }

        public static DemolishVisual Create(DemolishVisual prefab, IBuilding building)
        {
            if (!prefab || building == null)
                return null;

            var visual = Instantiate(prefab, building.WorldCenter, Quaternion.identity);
            visual.transform.localScale = new Vector3(building.Size.x, 1, building.Size.y);

            building.Pivot.SetParent(visual.Pivot, true);

            return visual;
        }
    }
}