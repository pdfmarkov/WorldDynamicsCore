using CityBuilderCore;
using UnityEngine;

namespace CityBuilderTown
{
    /// <summary>
    /// view used for bars that take care of retrieving values on their own instead of using an <see cref="IWalkerValue"/>
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Views/" + nameof(ViewWalkerBarGeneric))]
    public class ViewWalkerBarGeneric : ViewWalkerBarBase
    {
        public override IWalkerValue WalkerValue => null;
    }
}