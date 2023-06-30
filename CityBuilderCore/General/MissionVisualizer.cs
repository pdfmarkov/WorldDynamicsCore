using UnityEngine;
using UnityEngine.SceneManagement;

namespace CityBuilderCore
{
    /// <summary>
    /// displays mission info in unity ui and provides methods for starting/continuing it
    /// </summary>
    public class MissionVisualizer : MonoBehaviour
    {
        public Mission Mission;

        [Tooltip("object gets activated if the mission has been finished")]
        public GameObject FinishedObject;
        [Tooltip("objects gets activated if the mission has a savegame and can be continued")]
        public GameObject ContinueObject;

        public TMPro.TMP_Text NameText;
        public TMPro.TMP_Text DescriptionText;
        public TMPro.TMP_Text WinConditionsText;

        private void Start()
        {
            UpdateVisuals();
        }

        private void Update()
        {
            updateWinConditions();
        }

        public void UpdateVisuals()
        {
            if (FinishedObject)
                FinishedObject.SetActive(Mission.GetFinished());
            if (ContinueObject)
                ContinueObject.SetActive(Mission.GetStarted());

            if (NameText)
                NameText.text = Mission.Name;
            if (DescriptionText)
                DescriptionText.text = Mission.Description;

            updateWinConditions();
        }

        public void StartMission()
        {
            SceneManager.LoadSceneAsync(Mission.SceneName).completed += o =>
            {
                Dependencies.Get<IMissionManager>().SetMissionParameters(new MissionParameters() { Mission = Mission });
            };
        }

        public void ContinueMission()
        {
            SceneManager.LoadSceneAsync(Mission.SceneName).completed += o =>
            {
                Dependencies.Get<IMissionManager>().SetMissionParameters(new MissionParameters() { Mission = Mission, IsContinue = true });
            };
        }

        public void LoadScene(string scene)
        {
            SceneManager.LoadSceneAsync(scene);
        }

        private void updateWinConditions()
        {
            if (!WinConditionsText)
                return;

            WinConditionsText.text = Mission.GetWinConditionText(Dependencies.Get<IScoresCalculator>());
        }
    }
}