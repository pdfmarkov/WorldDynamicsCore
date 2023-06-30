using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// helper for activating a view from a <see cref="UnityEngine.UI.Toggle"/>
    /// </summary>
    public class ViewActivator : TooltipOwnerBase
    {
        [Tooltip("the view that will be activated when SetViewActive(bool) is invoked by some UI item that was wired to it in the inspector")]
        public View View;

        public override string TooltipName => View.Name;
        
        public void SetViewActive(bool active)
        {
            var viewsManager = Dependencies.Get<IViewsManager>();

            if (active)
                viewsManager.ActivateView(View);
            else
                viewsManager.DeactivateView(View);
        }
    }
}