using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// multiplies another score
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(MultipliedScore))]
    public class MultipliedScore : Score
    {
        [Tooltip("the score that will be multiplied")]
        public Score Score;
        [Tooltip("the multiplier the other score will be multiplied with")]
        public float Multiplier;

        public override int Calculate() => Mathf.RoundToInt(Score.Calculate() * Multiplier);
    }
}