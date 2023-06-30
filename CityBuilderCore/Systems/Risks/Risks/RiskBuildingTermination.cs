using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// a risk that when executed terminates the building
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Risks/" + nameof(RiskBuildingTermination))]
    public class RiskBuildingTermination : Risk
    {
        public override void Execute(IRiskRecipient risker)
        {
            base.Execute(risker);

            risker.Building.Terminate();
        }
    }
}