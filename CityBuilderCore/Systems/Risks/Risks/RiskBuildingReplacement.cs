using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// a risk that when executed replaces its building with something else<br/>
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Risks/" + nameof(RiskBuildingReplacement))]
    public class RiskBuildingReplacement : Risk
    {
        [Tooltip("when the risk triggers the afflicted building will be replaced with this one")]
        public Building Replacement;

        public override void Execute(IRiskRecipient risker)
        {
            base.Execute(risker);

            risker.Building.Replace(Replacement.Info.GetPrefab(risker.Building.Index));
        }
    }
}