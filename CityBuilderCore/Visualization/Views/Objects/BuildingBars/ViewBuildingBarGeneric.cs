using CityBuilderCore;
using UnityEngine;

namespace CityBuilderTown
{
    /// <summary>
    /// view used for bars that take care of retrieving values on their own instead of using an <see cref="IBuildingValue"/>
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Views/" + nameof(ViewBuildingBarGeneric))]
    public class ViewBuildingBarGeneric : ViewBuildingBarBase
    {
        public override IBuildingValue BuildingValue => null;
    }
}