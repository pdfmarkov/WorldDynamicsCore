using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// category for bundling and filtering buildings(entertainment, religion, ....), mainly used in scores
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(BuildingCategory))]
    public class BuildingCategory : KeyedObject
    {
        [Tooltip("name used when refering to a singular building of the category('you need to build a X')")]
        public string NameSingular;
        [Tooltip("name used when refering to multiple buildings of the category('you need to build 10 Y')")]
        public string NamePlural;
        [Tooltip("collection of all the buildings in the category")]
        public BuildingInfo[] Buildings;

        public string GetName(int quantity)
        {
            if (quantity > 1)
                return $"{quantity} {NamePlural}";
            else
                return NameSingular;
        }
    }
}