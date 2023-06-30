using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// object that just contains a NavMesh area mask<br/>
    /// can be used as a tag for pathfinding(<see cref="WalkerInfo.PathTag"/>)<br/>
    /// so the walker only walks on certain NavMesh areas
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(WalkerAreaMask))]
    public class WalkerAreaMask : ScriptableObject
    {
        public int AreaMask = -1;
    }
}
