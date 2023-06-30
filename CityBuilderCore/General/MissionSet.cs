using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// some collection of buildings<br/>
    /// a set of all buildings in the game is needed by <see cref="ObjectRepository"/> so buildings can be found when a game gets loaded
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Sets/" + nameof(MissionSet))]
    public class MissionSet : KeyedSet<Mission> { }
}