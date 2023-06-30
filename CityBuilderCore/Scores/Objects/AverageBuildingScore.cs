using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// averages the values for different buildings<br/>
    /// can be used for assessing the quality of housing
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(AverageBuildingScore))]
    public class AverageBuildingScore : Score
    {
        [Tooltip(@"give building types a score that will be added up for buildings on the map and averaged out
ie hut=40 villa=100 >> 3xhut+1xvilla=180/3=60")]
        public BuildingEvaluation[] Evaluations;

        public override int Calculate()
        {
            int count = 0;
            int value = 0;

            foreach (var evaluation in Evaluations)
            {
                var evaluationCount = evaluation.GetCount();
                var evaluationValue = evaluation.GetValue();

                value += evaluationValue * evaluationCount;
                count += evaluationCount;
            }

            if (count == 0)
                return 0;
            else
                return Mathf.RoundToInt(value / (float)count);
        }
    }
}