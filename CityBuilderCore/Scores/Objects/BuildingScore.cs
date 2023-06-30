using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// counts how many buildings currently exist on the map
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(BuildingScore))]
    public class BuildingScore : Score
    {
        [Tooltip("the building to count")]
        public BuildingInfo Building;

        public override int Calculate()
        {
            return Dependencies.Get<IBuildingManager>().Count(Building);
        }
    }
}