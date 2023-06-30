using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// sums the values for different buildings<br/>
    /// for example monument scores are added together
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(SummedBuildingScore))]
    public class SummedBuildingScore : Score
    {
        [Tooltip(@"give building types a score that will be added up for buildings on the map
ie monolith=40 pyramid=100 >> 3xmonolith+1xpyramid=220")]
        public BuildingEvaluation[] Evaluations;

        public override int Calculate()
        {
            return Evaluations.Sum(i => i.GetEvaluation());
        }
    }
}