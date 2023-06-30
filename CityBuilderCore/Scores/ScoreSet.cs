using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// some collection of scores<br/>
    /// a set of all scores in the game is needed by <see cref="ObjectRepository"/> so the <see cref="IScoresCalculator"/> can calculate them
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Sets/" + nameof(ScoreSet))]
    public class ScoreSet : ObjectSet<Score> { }
}