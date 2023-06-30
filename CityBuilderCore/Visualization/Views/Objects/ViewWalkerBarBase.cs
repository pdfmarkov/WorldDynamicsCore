using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// non generic base class so <see cref="ViewWalkerBar{T}"/> can be accessed in <see cref="IBarManager"/>
    /// </summary>
    public abstract class ViewWalkerBarBase : View
    {
        [Tooltip("prefab of the bar that will be instantiated on walkers")]
        public WalkerValueBar Bar;

        public abstract IWalkerValue WalkerValue { get; }

        public override void Activate() => Dependencies.Get<IBarManager>().ActivateBar(this);
        public override void Deactivate() => Dependencies.Get<IBarManager>().DeactivateBar(this);
    }
}