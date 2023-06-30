using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// some collection of addons<br/>
    /// a set of all addons in the game is needed by <see cref="ObjectRepository"/> so addons can be found when a game gets loaded
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Sets/" + nameof(BuildingAddonSet))]
    public class BuildingAddonSet : KeyedSet<BuildingAddon> { }
}