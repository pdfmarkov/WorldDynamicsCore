namespace CityBuilderCore
{
    public interface IMissionManager
    {
        /// <summary>
        /// gets mission, difficulty, new/continue of the active mission
        /// </summary>
        MissionParameters MissionParameters { get; }

        /// <summary>
        /// sets mission, difficulty, new/continue<br/>
        /// triggers mission initialization like loading for continue
        /// </summary>
        /// <param name="missionParameters"></param>
        void SetMissionParameters(MissionParameters missionParameters);
    }
}