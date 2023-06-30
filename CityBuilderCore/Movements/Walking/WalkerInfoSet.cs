using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// some collection of walkers<br/>
    /// a set of all walkers in the game is needed by <see cref="ObjectRepository"/> so walkers can be found when a game gets loaded
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Sets/" + nameof(WalkerInfoSet))]
    public class WalkerInfoSet : ObjectSet<WalkerInfo> { }
}