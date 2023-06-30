using UnityEngine;
using UnityEngine.SceneManagement;

namespace CityBuilderCore
{
    /// <summary>
    /// dialog for mission stuff<br/>
    /// just a wrapper for <see cref="MissionVisualizer"/>
    /// </summary>
    public class MissionDialog : DialogBase
    {
        [Tooltip("the visualizer that actually displays the mission parameters, the dialog makes sure it gets the current mission")]
        public MissionVisualizer MissionVisualizer;
        [Tooltip("scene that will be loaded if exit is clicked")]
        public string ExitSceneName;

        public override void Activate()
        {
            base.Activate();

            MissionVisualizer.Mission = Dependencies.Get<IMissionManager>().MissionParameters.Mission;
            MissionVisualizer.UpdateVisuals();
        }

        public void ExitMission()
        {
            SceneManager.LoadSceneAsync(ExitSceneName);
        }
    }
}