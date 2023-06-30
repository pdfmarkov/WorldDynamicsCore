using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// some collection of employment groups<br/>
    /// a set of all groups in the game is needed in <see cref="ObjectRepository"/> for the population manager to find priorities
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Sets/" + nameof(EmploymentGroupSet))]
    public class EmploymentGroupSet : KeyedSet<EmploymentGroup> { }
}