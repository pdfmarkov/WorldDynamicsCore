using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// an event that, on activation, modifies the risk values of a set amount of buildings<br/>
    /// increase > arsonist, disease outbreak, natural disaster(volcano, earthquake) ...<br/>
    /// decrease > blessings, ...
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Happenings/" + nameof(RiskModificationHappening))]
    public class RiskModificationHappening : TimingHappening
    {
        [Tooltip("the risk that will be modified when the happening starts")]
        public Risk Risk;
        [Tooltip("amount that the risk will be modified by(positive to increase and maybe trigger risks, negative to reduce)")]
        public float Amount;
        [Tooltip("how many randomly selected building will be affected, 0 or less for all")]
        public int Count;

        public override void Start()
        {
            base.Start();

            if (Count > 0)
            {
                foreach (var building in Dependencies.Get<IBuildingManager>().GetRandom(Count, b => Risk.HasValue(b)))
                {
                    Risk.ModifyValue(building, Amount);
                }
            }
            else
            {
                foreach (var building in Dependencies.Get<IBuildingManager>().GetBuildings().Where(b => Risk.HasValue(b)))
                {
                    Risk.ModifyValue(building, Amount);
                }
            }
        }
    }
}
