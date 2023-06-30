using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// view that displays an overlay for efficiency on a tilemap
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Views/" + nameof(ViewEfficiency))]
    public class ViewEfficiency : View
    {
        [Tooltip("gradient for building efficiency from 0 to 1 that will be displayed as an overlay")]
        public Gradient Gradient;

        public override void Activate() => Dependencies.Get<IOverlayManager>().ActivateOverlay(this);
        public override void Deactivate() => Dependencies.Get<IOverlayManager>().ClearOverlay();
    }
}
