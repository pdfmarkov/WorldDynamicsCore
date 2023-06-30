using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// blank building addon that can be used to attach particle effects for example<br/>
    /// removal is either done when the building is replaced(Evolution went through) or from the outside(evolution canceled)
    /// </summary>
    public class BuildingAddonEffect : BuildingAddon
    {
        [Tooltip("addon will not carry over when its building is replaced")]
        public bool RemoveOnReplace;

        public override void OnReplacing(Transform parent, IBuilding replacement)
        {
            if (!RemoveOnReplace)
                base.OnReplacing(parent, replacement);
        }
    }
}