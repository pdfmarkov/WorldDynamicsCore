using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// kills a part of the population on start(relative to the capacity of the housing)
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Happenings/" + nameof(DepopulationHappening))]
    public class DepopulationHappening : TimingHappening
    {
        [Tooltip("ratio of the population to kill")]
        [Range(0, 1)]
        public float Mortality;

        public override void Start()
        {
            base.Start();

            Dependencies.Get<IPopulationManager>().GetHousings().ForEach(h => h.Kill(Mortality));
        }
    }
}
