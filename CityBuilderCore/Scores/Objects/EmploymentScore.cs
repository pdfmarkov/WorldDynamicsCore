using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// employment percentage for a certain population
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(EmploymentScore))]
    public class EmploymentScore : Score
    {
        [Tooltip("the population for which  for calculate employment percentage")]
        public Population Population;
        [Tooltip("whether the result should be clamped between 0 and 100")]
        public bool Clamped;

        public override int Calculate()
        {
            var value = Mathf.RoundToInt(Dependencies.Get<IEmploymentManager>().GetEmploymentRate(Population) * 100f);
            if (Clamped)
                value = Mathf.Clamp(value, 0, 100);
            return value;
        }
    }
}