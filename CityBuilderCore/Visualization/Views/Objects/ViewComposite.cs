using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// view that transfers activation to its children
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Views/" + nameof(ViewComposite))]
    public class ViewComposite : View
    {
        [Tooltip("the sub-views that will be activated/deactivated with this view")]
        public View[] Views;

        public override void Activate() => Views.ForEach(v => v.Activate());
        public override void Deactivate() => Views.ForEach(v => v.Deactivate());
    }
}