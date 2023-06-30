using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// bundels of risks for whenever instead of a specific risk just a general type of risk is needed<br/>
    /// useful mainly for views and visualizations that encompass multiple risks
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(RiskCategory))]
    public class RiskCategory : KeyedObject, IBuildingValue, IWalkerValue
    {
        [Tooltip("name when refering to one risk(hazard)")]
        public string NameSingular;
        [Tooltip("name when refering to multiple risks(2 hazards)")]
        public string NamePlural;
        [Tooltip("then risks that are part of this category")]
        public Risk[] Risks;

        public string GetName(int quantity)
        {
            if (quantity > 1)
                return $"{quantity} {NamePlural}";
            else
                return NameSingular;
        }

        public bool HasValue(IBuilding building) => building.GetBuildingComponents<IBuildingComponent>().OfType<IRiskRecipient>().Any(c => Risks.Any(r => c.HasRiskValue(r)));
        public float GetMaximum(IBuilding building) => Risks.Length * 100f;
        public float GetValue(IBuilding building) => building.GetBuildingComponents<IBuildingComponent>().OfType<IRiskRecipient>().Sum(c => Risks.Where(r => c.HasRiskValue(r)).Sum(s => c.GetRiskValue(s)));
        public Vector3 GetPosition(IBuilding building) => building.WorldCenter;

        public bool HasValue(Walker walker) => walker is RiskWalker riskWalker && Risks.Contains(riskWalker.Risk);
        public float GetMaximum(Walker _) => 1f;
        public float GetValue(Walker walker) => HasValue(walker) ? 1f : 0f;
        public Vector3 GetPosition(Walker walker) => walker.Pivot.position;
    }
}