using System;

namespace CityBuilderCore
{
    /// <summary>
    /// contains all parameters needed to start a scene<br/>
    /// additionally to the <see cref="Mission"/> which contains what to start this defines how it is started<br/>
    /// </summary>
    [Serializable]
    public class MissionParameters
    {
        public Mission Mission;
        public Difficulty Difficulty;
        public bool IsContinue;
        public string ContinueName;
        public int RandomSeed;

        /// <summary>
        /// has the mission been started/has a savegame been found at the specified difficulty
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool GetStarted() => Mission.GetStarted(Difficulty);
        /// <summary>
        /// has the mission been finished at the specified difficulty
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool GetFinished() => Mission.GetFinished(Difficulty);
        /// <summary>
        /// are all the win conditions currently satisfied
        /// </summary>
        /// <param name="calculator"></param>
        /// <returns></returns>
        public bool GetWon(IScoresCalculator calculator) => Mission.GetWon(calculator);

        public string Load(string name = null) => Mission.Load(name, Difficulty);
        public void Save(string data, string name = null) => Mission.Save(data, name, Difficulty);
    }
}