using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// some collection of roads, a set of all roads should be set in <see cref="ObjectRepository"/>
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Sets/" + nameof(RoadSet))]
    public class RoadSet : KeyedSet<Road> { }
}