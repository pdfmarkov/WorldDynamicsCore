using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CityBuilderCore
{
    /// <summary>
    /// default implementation for various meta game functionality(missions, saving, settings, ...)
    /// </summary>
    public class DefaultGameManager : MonoBehaviour, IMissionManager, IGameSpeed, IGameSettings, IGameSaver, IDifficultyFactor
    {
        [Header("Game")]
        [Tooltip("when false efficiency is disabled and always 1, mainly useful for debugging")]
        public bool DisableEfficiency = false;
        [Tooltip("when false employment is disabled and behaves as if always fully employed, mainly useful for debugging")]
        public bool DisableEmployment = false;
        [Tooltip("how often various checkers(this.StartChecker) in the game run(calculating scores, check immigration, check employment, ...)")]
        public float CheckInterval = 1f;
        [Tooltip("current game speed, is applied to Time.timeScale so unscaledTime is not affected")]
        public float Speed = 1f;
        [Tooltip("pause/resume game, overrides Speed to be 0")]
        public bool IsPaused = false;
        [Tooltip("mission used when a scene is started directly without receiving its mission from a menu, useful for debugging mission screens and such")]
        public MissionParameters DebugMission;
        [Header("Difficulty")]
        [Tooltip("influences the speed at which risks increase")]
        public float RiskMultiplier = 1f;
        [Tooltip("influences the speed at which services deplete")]
        public float ServiceMultiplier = 1f;
        [Tooltip("influences the speed at which items deplete")]
        public float ItemsMultiplier = 1f;
        [Header("Saving")]
        public bool SaveMetaData;

        bool IGameSettings.HasEfficiency => !DisableEfficiency;
        bool IGameSettings.HasEmployment => !DisableEmployment;
        float IGameSettings.RiskMultiplier => _currentRiskMultiplier;
        float IGameSettings.ServiceMultiplier => _currentServiceMultiplier;
        float IGameSettings.ItemsMultiplier => _currentItemsMultiplier;
        float IGameSettings.CheckInterval => CheckInterval;
        bool IGameSpeed.IsPaused => IsPaused;

        /// <summary>
        /// fired true at the start of saving and false at the end, might be used to display save messages
        /// </summary>
        public BoolEvent IsSavingChanged;
        /// <summary>
        /// fired true at the start of loading and false at the end, might be used to display loading messages
        /// </summary>
        public BoolEvent IsLoadingChanged;
        /// <summary>
        /// fired when a mission is started for the first time, might be used to display a mission briefing
        /// </summary>
        public UnityEvent Started;
        /// <summary>
        /// fired when the missions win conditions are satisfied for the first time, might be used to display a win screen
        /// </summary>
        public UnityEvent Finished;
        /// <summary>
        /// fired when a happening starts or ends
        /// </summary>
        public TimingHappeningStateEvent HappeningStateChanged;

        public bool IsSaving { get; private set; }
        public bool IsLoading { get; private set; }

        float IDifficultyFactor.RiskMultiplier => RiskMultiplier;
        float IDifficultyFactor.ServiceMultiplier => ServiceMultiplier;
        float IDifficultyFactor.ItemsMultiplier => ItemsMultiplier;

        public float Playtime => Time.time - _loadTime + _previousTime;
        public MissionParameters MissionParameters { get; private set; }

        private float _speed = 1f;
        private bool _isPaused = false;

        private float _loadTime = 0f;
        private float _previousTime = 0f;

        private List<IDifficultyFactor> _difficultyFactors = new List<IDifficultyFactor>();
        private float _currentRiskMultiplier;
        private float _currentServiceMultiplier;
        private float _currentItemsMultiplier;

        private ExtraDataBehaviour[] _extras;

        private TimingHappeningState[] _happeningStates;

        protected virtual void Awake()
        {
            Dependencies.Register<IMissionManager>(this);
            Dependencies.Register<IGameSaver>(this);
            Dependencies.Register<IGameSpeed>(this);
            Dependencies.Register<IGameSettings>(this);

            _extras = FindObjectsOfType<ExtraDataBehaviour>();
        }

        protected virtual void Start()
        {
            RegisterDifficultyFactor(this);

            if (MissionParameters == null)
            {
                if (DebugMission.Mission)
                {
                    SetMissionParameters(DebugMission);
                }
                else
                {
                    Started?.Invoke();
                    _loadTime = Time.time;
                }
            }

            setTimeScale();
        }

        protected virtual void Update()
        {
            calculateDifficulty();

            if (Speed != _speed)
            {
                _speed = Speed;
                setTimeScale();
            }

            if (IsPaused != _isPaused)
            {
                _isPaused = IsPaused;
                setTimeScale();
            }
        }

        public void SetMissionParameters(MissionParameters missionParameters)
        {
            MissionParameters = missionParameters;
            if (MissionParameters.Difficulty != null)
                RegisterDifficultyFactor(MissionParameters.Difficulty);

            if (MissionParameters.IsContinue)
            {
                LoadNamed(MissionParameters.ContinueName);
            }
            else
            {
                _loadTime = Time.time;
                startHappenings();
                UnityEngine.Random.InitState(missionParameters.RandomSeed);
                if (MissionParameters != DebugMission)
                    Started?.Invoke();
            }

            if (MissionParameters.Mission.Happenings.Length > 0)
                this.StartChecker(checkHappenings);

            if (!MissionParameters.Mission.GetFinished() && MissionParameters.Mission.HasWinConditions)
                this.StartChecker(checkWin);
        }

        public void Restart()
        {
            SceneManager.LoadSceneAsync(gameObject.scene.name).completed += o =>
            {
                if (MissionParameters?.Mission != null)
                    Dependencies.Get<IMissionManager>().SetMissionParameters(new MissionParameters() { Mission = MissionParameters.Mission });
            };
        }

        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
        public void SetSpeed(float speed)
        {
            IsPaused = false;
            Speed = speed;
        }

        public void RegisterDifficultyFactor(IDifficultyFactor difficultyFactor) => _difficultyFactors.Add(difficultyFactor);
        public void DeregisterDifficultyFactor(IDifficultyFactor difficultyFactor) => _difficultyFactors.Remove(difficultyFactor);

        private void calculateDifficulty()
        {
            _currentRiskMultiplier = 1f;
            _currentServiceMultiplier = 1f;
            _currentItemsMultiplier = 1f;

            foreach (var difficultyFactor in _difficultyFactors)
            {
                _currentRiskMultiplier *= difficultyFactor.RiskMultiplier;
                _currentServiceMultiplier *= difficultyFactor.ServiceMultiplier;
                _currentItemsMultiplier *= difficultyFactor.ItemsMultiplier;
            }
        }

        private void setTimeScale()
        {
            if (IsPaused)
                Time.timeScale = 0f;
            else
                Time.timeScale = Speed;
        }

        private void startHappenings()
        {
            if (MissionParameters?.Mission?.Happenings == null || MissionParameters.Mission.Happenings.Length == 0)
                return;

            _happeningStates = MissionParameters.Mission.Happenings.Select(h => new TimingHappeningState(h)).ToArray();
            foreach (var state in _happeningStates)
            {
                state.Start(Playtime, MissionParameters.RandomSeed);
            }
        }
        private void checkHappenings()
        {
            if (_happeningStates == null)
                return;

            foreach (var state in _happeningStates)
            {
                if (state.Check(Playtime, MissionParameters.RandomSeed))
                    HappeningStateChanged?.Invoke(state);
            }
        }
        private void endHappenings()
        {
            if (_happeningStates == null)
                return;

            foreach (var state in _happeningStates)
            {
                state.End();
            }
            _happeningStates = null;
        }

        private bool checkWin()
        {
            if (!MissionParameters.Mission.GetWon(Dependencies.Get<IScoresCalculator>()))
                return true;

            SaveHelper.Finish(MissionParameters.Mission.Key);
            Finished?.Invoke();
            return false;
        }

        #region Saving
        [Serializable]
        public class SaveData
        {
            public string Version;
            public float Playtime;
            public UnityEngine.Random.State RandomState;
            public Vector3 CameraPosition;
            public float CameraSize;
            public Quaternion CameraRotation;
            public string StructuresData;
            public string RoadsData;
            public string BuildingsData;
            public string PopulationData;
            public string EmploymentData;
            public ItemStorage.ItemStorageData GlobalStorageData;
            public ExtraSaveData[] Extras;
        }
        [Serializable]
        public class ExtraSaveData
        {
            public string Key;
            public string Data;
        }
        [Serializable]
        public class SaveDataMeta
        {
            public byte[] Image;
            public long SavedAt;
            public float Playtime;
        }

        public void Save()
        {
            if (IsSaving || IsLoading)
                return;

            StartCoroutine(save());
        }
        public void SaveNamed(string name)
        {
            if (IsSaving || IsLoading)
                return;

            StartCoroutine(save(name));
        }
        private IEnumerator save(string name = null)
        {
            IsSaving = true;
            IsSavingChanged?.Invoke(true);

            var speed = Dependencies.Get<IGameSpeed>();
            var wasPaused = speed.IsPaused;

            if (!wasPaused)
                speed.Pause();

            yield return null;

            if (SaveMetaData)
            {
                var metaData = new SaveDataMeta()
                {
                    SavedAt = DateTime.Now.ToFileTime(),
                    Playtime = Playtime,
                    Image = ScreenCapture.CaptureScreenshotAsTexture().Scale(128, 128).EncodeToPNG()
                };

                SaveHelper.SetExtra(SaveHelper.GetKey(MissionParameters?.Mission, MissionParameters?.Difficulty), name, "META", JsonUtility.ToJson(metaData));
            }

            var data = new SaveData()
            {
                Version = Application.version,
                Playtime = Playtime,
                RandomState = UnityEngine.Random.state,
                CameraPosition = Dependencies.Get<IMainCamera>().Position,
                CameraSize = Dependencies.Get<IMainCamera>().Size,
                CameraRotation = Dependencies.Get<IMainCamera>().Rotation,
                StructuresData = Dependencies.GetOptional<IStructureManager>()?.SaveData(),
                RoadsData = Dependencies.GetOptional<IRoadManager>()?.SaveData(),
                BuildingsData = Dependencies.GetOptional<IBuildingManager>()?.SaveData(),
                PopulationData = Dependencies.GetOptional<IPopulationManager>()?.SaveData(),
                EmploymentData = Dependencies.GetOptional<IEmploymentManager>()?.SaveData(),
                GlobalStorageData = Dependencies.GetOptional<IGlobalStorage>()?.Items.SaveData(),
                Extras = _extras.Select(e => new ExtraSaveData() { Key = e.Key, Data = e.SaveData() }).ToArray()
            };

            SaveHelper.Save(SaveHelper.GetKey(MissionParameters?.Mission, MissionParameters?.Difficulty), name, JsonUtility.ToJson(data));

            yield return null;

            if (!wasPaused)
                Dependencies.Get<IGameSpeed>().Resume();

            yield return null;

            IsSaving = false;
            IsSavingChanged?.Invoke(false);
        }

        public void Load()
        {
            if (IsSaving || IsLoading)
                return;

            StartCoroutine(load());
        }
        public void LoadNamed(string name)
        {
            if (IsSaving || IsLoading)
                return;

            StartCoroutine(load(name));
        }
        private IEnumerator load(string name = null)
        {
            IsLoading = true;
            IsLoadingChanged?.Invoke(true);

            Dependencies.Get<IGameSpeed>().Pause();
            endHappenings();

            yield return null;

            SaveData data;

            try
            {
                data = JsonUtility.FromJson<SaveData>(SaveHelper.Load(SaveHelper.GetKey(MissionParameters?.Mission, MissionParameters?.Difficulty), name));
            }
            catch
            {
                data = null;
            }

            if (data == null)
                yield break;

            UnityEngine.Random.state = data.RandomState;

            Dependencies.Get<IMainCamera>().Position = data.CameraPosition;
            Dependencies.Get<IMainCamera>().Size = data.CameraSize;
            Dependencies.Get<IMainCamera>().Rotation = data.CameraRotation;

            _loadTime = Time.time;
            _previousTime = data.Playtime;

            Dependencies.GetOptional<IStructureManager>()?.LoadData(data.StructuresData);
            Dependencies.GetOptional<IRoadManager>()?.LoadData(data.RoadsData);
            Dependencies.GetOptional<IBuildingManager>()?.LoadData(data.BuildingsData);
            Dependencies.GetOptional<IPopulationManager>()?.LoadData(data.PopulationData);
            Dependencies.GetOptional<IEmploymentManager>()?.LoadData(data.EmploymentData);
            Dependencies.GetOptional<IGlobalStorage>()?.Items.LoadData(data.GlobalStorageData);

            if (data.Extras != null)
            {
                foreach (var extraData in data.Extras)
                {
                    var extraBehaviour = _extras.FirstOrDefault(e => e.Key == extraData.Key);
                    if (!extraBehaviour)
                        continue;
                    extraBehaviour.LoadData(extraData.Data);
                }
            }

            yield return null;

            Dependencies.GetOptional<IEmploymentManager>()?.CheckEmployment();

            yield return null;

            startHappenings();
            Dependencies.Get<IGameSpeed>().Resume();

            yield return null;

            IsLoading = false;
            IsLoadingChanged?.Invoke(false);
        }
        #endregion
    }
}