using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// view that changes camera culling<br/>
    /// eg hiding irrelevant buildings 
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Views/" + nameof(ViewCulling))]
    public class ViewCulling : View
    {
        [Tooltip("the culling that will be set on the main camera when this view is active")]
        public LayerMask Culling;

        public override void Activate() => Dependencies.Get<IMainCamera>().SetCulling(Culling);
        public override void Deactivate() => Dependencies.Get<IMainCamera>().ResetCulling();
    }
}