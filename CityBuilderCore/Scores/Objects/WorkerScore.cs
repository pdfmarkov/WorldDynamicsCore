using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// worker percentage for a certain population<br/>
    /// available/needed >> 20xvillager/10xfarmers=200%
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(WorkerScore))]
    public class WorkerScore : Score
    {
        [Tooltip(@"the kind of population for which to calculate worker percentage
available/needed >> 20xvillager/10xfarmers=200%")]
        public Population Population;
        [Tooltip("whether the result will be clamped between 0 and 100")]
        public bool Clamped;

        public override int Calculate()
        {
            var value=Mathf.RoundToInt(Dependencies.Get<IEmploymentManager>().GetWorkerRate(Population) * 100f);
            if (Clamped)
                value = Mathf.Clamp(value, 0, 100);
            return value;
        }
    }
}