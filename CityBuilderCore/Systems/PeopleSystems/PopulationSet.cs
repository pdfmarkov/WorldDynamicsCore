using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// some collection of populations<br/>
    /// a set of all populations in the game is needed in <see cref="ObjectRepository"/> so populations can be found when a game gets loaded
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Sets/" + nameof(PopulationSet))]
    public class PopulationSet : KeyedSet<Population> { }
}