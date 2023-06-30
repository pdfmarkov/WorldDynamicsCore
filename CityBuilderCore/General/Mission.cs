using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// object that defines general information about a level
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(Mission))]
    public class Mission : KeyedObject
    {
        [Tooltip("display name")]
        public string Name;
        [Tooltip("display description")]
        [TextArea]
        public string Description;
        [Tooltip("scene that should be loaded for the mission")]
        public string SceneName;
        [Tooltip("happenings that will occur at their specified time in this mission")]
        public TimingHappeningOccurence[] Happenings;
        [Tooltip("condition that have to be satisified before the mission counts as won, leave empty for endless missions")]
        public WinCondition[] WinConditions;

        /// <summary>
        /// does the mission have win conditions or is it endless
        /// </summary>
        public bool HasWinConditions => WinConditions != null && WinConditions.Length > 0;

        /// <summary>
        /// has the mission been started/has a savegame been found at the specified difficulty
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool GetStarted(Difficulty difficulty = null)
        {
            return SaveHelper.HasSave(getKey(difficulty), null);
        }

        /// <summary>
        /// has the mission been finished at the specified difficulty
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool GetFinished(Difficulty difficulty = null)
        {
            return SaveHelper.GetFinished(getKey(difficulty));
        }

        public List<string> GetSaves(Difficulty difficulty = null) => SaveHelper.GetSaves(getKey(difficulty));

        /// <summary>
        /// are all the win conditions currently satisfied
        /// </summary>
        /// <param name="calculator"></param>
        /// <returns></returns>
        public bool GetWon(IScoresCalculator calculator)
        {
            foreach (var condition in WinConditions)
            {
                if (calculator.GetValue(condition.Score) < condition.Value)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// progress of all the win conditions in text form
        /// </summary>
        /// <param name="calculator"></param>
        /// <returns></returns>
        public string GetWinConditionText(IScoresCalculator calculator)
        {
            StringBuilder sb = new StringBuilder();

            if (WinConditions != null)
            {
                foreach (var winCondition in WinConditions)
                {
                    sb.AppendLine($"{winCondition.Score.Name} {calculator.GetValue(winCondition.Score)}/{winCondition.Value}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// loads savedata as string
        /// </summary>
        /// <param name="name">name of the savegame or null for quicksave</param>
        /// <param name="difficulty">difficulty or null for default</param>
        /// <returns></returns>
        public string Load(string name = null, Difficulty difficulty = null)
        {
            return SaveHelper.Load(Key + (difficulty ? difficulty.Key : string.Empty), name);
        }

        /// <summary>
        /// saves the data string
        /// </summary>
        /// <param name="name">name of the savegame or null for quicksave</param>
        /// <param name="difficulty">difficulty or null for default</param>
        /// <param name="difficulty"></param>
        public void Save(string data, string name = null, Difficulty difficulty = null)
        {
            SaveHelper.Save(getKey(difficulty), name, data);
        }

        /// <summary>
        /// deletes the save game
        /// </summary>
        /// <param name="name">name of the save game or null for quicksave</param>
        /// <param name="difficulty">difficulty or null for default</param>
        public void Delete(string name = null, Difficulty difficulty = null)
        {
            SaveHelper.Delete(getKey(difficulty), name);
        }

        private string getKey(Difficulty difficulty) => SaveHelper.GetKey(this, difficulty);
    }
}