using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace CityBuilderCore
{
    /// <summary>
    /// base class for entites moving about the map<br/>
    /// typically created by some kind of <see cref="WalkerSpawner{T}"/> on a building<br/>
    /// some other ways to create walkers are having spawners on a global manager(urban, town)<br/>
    /// or even instantiating and managing the walker save data yourself(<see cref="TilemapSpawner"/> used in defense)
    /// </summary>
    public abstract class Walker : MonoBehaviour, ISaveData, IOverrideHeight
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public BuildingReference Home { get; set; }
        public ProcessState CurrentProcess { get; set; }
        public RoamingState CurrentRoaming { get; set; }
        public WalkingState CurrentWalking { get; set; }
        public WalkingAgentState CurrentAgentWalking { get; set; }
        public WaitingState CurrentWaiting { get; set; }

        public float? HeightOverride { get; set; }

        [Tooltip("contains all the meta info that is relevant to every walker of this type like speed or pathing")]
        public WalkerInfo Info;
        [Tooltip("important transform used in rotation and variance, should contain all visuals")]
        public Transform Pivot;
        [Tooltip("used for the paramters in WalkerInfo and when starting animations from WalkerActions(optional)")]
        public Animator Animator;
        [Tooltip("optional agent that can be used to get to the destination when using PathType.Map, otherwise the walker strictly follows the calculated path")]
        public NavMeshAgent Agent;

        public Vector2Int CurrentPoint => _current;
        public Vector2Int GridPoint => Dependencies.Get<IGridPositions>().GetGridPosition(Pivot.position);

        public virtual float Speed => Info.Speed;
        public virtual PathType PathType => Info.PathType;
        public virtual UnityEngine.Object PathTag => Info.PathTagSelf ? Info : Info.PathTag;

        public virtual ItemStorage ItemStorage => null;

        public WalkerAction CurrentAction => CurrentProcess?.CurrentAction;

        private bool _isWalking;
        public bool IsWalking
        {
            get { return _isWalking; }
            set
            {
                _isWalking = value;
                onIsWalkingChanged(value);
            }
        }

        public BoolEvent IsWalkingChanged;
        public Vector3Event DirectionChanged;

        public event Action<Walker> Finished;
        public event Action<Walker> Moved;

        protected Vector2Int _start;
        protected Vector2Int _current;
        protected bool _isFinished;

        private bool _isLoaded;

        protected virtual void Start()
        {
            if (!_isLoaded && Pivot)
                Pivot.localPosition += Dependencies.Get<IMap>().GetVariance();

            if (ItemStorage != null)
            {
                ItemStorage.Changed += onItemStorageChanged;
                onItemStorageChanged(ItemStorage);
            }
        }

        protected virtual void OnDestroy()
        {
            if (!gameObject.scene.isLoaded)
                return;

            //when the walker gets destroy without having finished(reload, attack, ...)
            Dependencies.Get<IWalkerManager>().DeregisterWalker(this);
        }

        /// <summary>
        /// called right after instantiating or reactivating a walker<br/>
        /// buildings have not had a chance to interact with the walker<br/>
        /// when your logic somthing from outside first override <see cref="Spawned"/> instead
        /// </summary>
        /// <param name="home"></param>
        /// <param name="start"></param>
        public virtual void Initialize(BuildingReference home, Vector2Int start)
        {
            _start = start;
            _current = start;
            Home = home;

            IsWalking = false;

            Dependencies.Get<IWalkerManager>().RegisterWalker(this);
        }

        /// <summary>
        /// called after the walker is fully initialized and its spawning has been signaled to the owner
        /// </summary>
        public virtual void Spawned() { }

        public void StartProcess(WalkerAction[] actions, string key = null)
        {
            CurrentProcess?.Cancel(this);
            CurrentProcess = new ProcessState(key, actions);
            CurrentProcess.Start(this);
        }
        public void AdvanceProcess()
        {
            if (CurrentProcess.Advance(this))
                return;
            var process = CurrentProcess;
            CurrentProcess = null;

            onProcessFinished(process);
        }
        public void CancelProcess()
        {
            CurrentProcess?.Cancel(this);
        }
        protected internal void continueProcess()
        {
            CurrentProcess.Continue(this);
        }
        protected virtual void onProcessFinished(ProcessState process)
        {
            onFinished();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            try
            {
                string debugText = GetDebugText();
                if (string.IsNullOrWhiteSpace(debugText))
                    return;

                UnityEditor.Handles.Label(Pivot.position, debugText);
            }
            catch
            {
                //dont care
            }
        }
#endif

        private void OnDrawGizmosSelected()
        {
            if (CurrentWalking?.WalkingPath != null)
            {
                Gizmos.color = Color.white;

                for (int i = 0; i < CurrentWalking.WalkingPath.Length - 1; i++)
                {
                    Gizmos.DrawLine(CurrentWalking.WalkingPath.GetPreviousPosition(i), CurrentWalking.WalkingPath.GetNextPosition(i));
                }
            }
        }

        protected virtual void onMoved(Vector2Int point)
        {
            _current = point;
            Moved?.Invoke(this);
        }

        protected virtual void onFinished()
        {
            _isFinished = true;

            Dependencies.Get<IWalkerManager>().DeregisterWalker(this);

            Finished?.Invoke(this);
            if (ItemStorage != null)
                ItemStorage.Clear();
            if (gameObject && gameObject.activeSelf)
                Destroy(gameObject);//in case walker was not released by spawner
        }

        public void roam(int memoryLength, int range, PathType pathType, UnityEngine.Object pathTag, Action finished = null)
        {
            StartCoroutine(WalkingPath.Roam(this, Info.Delay, _current, memoryLength, range, pathType, pathTag, finished ?? onFinished, onMoved));
        }
        public void roam(int memoryLength, int range, Action finished = null)
        {
            StartCoroutine(WalkingPath.Roam(this, Info.Delay, _current, memoryLength, range, PathType, PathTag, finished ?? onFinished, onMoved));
        }
        public void roam(float delay, int memoryLength, int range, Action finished = null)
        {
            StartCoroutine(WalkingPath.Roam(this, delay, _current, memoryLength, range, PathType, PathTag, finished ?? onFinished, onMoved));
        }
        public void continueRoam(int memoryLength, int range, Action finished)
        {
            StartCoroutine(WalkingPath.ContinueRoam(this, memoryLength, range, PathType, PathTag, finished ?? onFinished, onMoved));
        }
        public void continueRoam(int memoryLength, int range, PathType pathType, UnityEngine.Object pathTag, Action finished)
        {
            StartCoroutine(WalkingPath.ContinueRoam(this, memoryLength, range, pathType, pathTag, finished ?? onFinished, onMoved));
        }
        public void cancelRoam()
        {

        }

        public bool walk(IBuilding target, Action finished = null)
        {
            var path = PathHelper.FindPath(_current, target, PathType, PathTag);
            if (path == null)
                return false;
            walk(path, finished);
            return true;
        }
        public bool walk(Vector2Int target, Action finished = null)
        {
            return walk(target, PathType, PathTag, finished);
        }
        public bool walk(Vector2Int target, PathType pathType, UnityEngine.Object pathTag, Action finished = null)
        {
            var path = PathHelper.FindPath(_current, target, pathType, pathTag);
            if (path == null)
                return false;

            if (pathType == PathType.Map && Agent)
                StartCoroutine(path.WalkAgent(this, Info.Delay, finished ?? onFinished, onMoved));
            else
                StartCoroutine(path.Walk(this, Info.Delay, finished ?? onFinished, onMoved));

            return true;
        }
        public void walk(WalkingPath path, Action finished = null)
        {
            walk(path, Info.Delay, finished);
        }
        public void walk(WalkingPath path, float delay, Action finished = null)
        {
            if (PathType == PathType.Map && Agent)
                StartCoroutine(path.WalkAgent(this, delay, finished ?? onFinished, onMoved));
            else
                StartCoroutine(path.Walk(this, delay, finished ?? onFinished, onMoved));
        }
        public void continueWalk(Action finished = null)
        {
            if (Agent && CurrentAgentWalking != null)
                StartCoroutine(CurrentWalking.WalkingPath.ContinueWalkAgent(this, finished ?? onFinished, onMoved));
            else
                StartCoroutine(CurrentWalking.WalkingPath.ContinueWalk(this, finished ?? onFinished, onMoved));
        }
        public void cancelWalk()
        {
            if (Agent && CurrentAgentWalking != null)
                CurrentAgentWalking.Cancel();
            else
                CurrentWalking.Cancel();
        }

        public void wander(Action finished)
        {
            var adjacent = PathHelper.GetAdjacent(CurrentPoint, PathType, PathTag);
            if (!adjacent.Any())
            {
                finished();
                return;
            }

            walk(new WalkingPath(new[] { CurrentPoint, adjacent.Random() }), finished);
        }
        public void continueWander(Action finished)
        {
            continueWalk(finished);
        }

        public void wait(Action finished, float time)
        {
            StartCoroutine(waitRoutine(finished, time));
        }
        public void delay(Action finished)
        {
            StartCoroutine(waitRoutine(finished, Info.Delay));
        }
        private IEnumerator waitRoutine(Action finished, float time)
        {
            CurrentWaiting = new WaitingState(time);
            yield return continueWaitRoutine(finished);
        }
        public void continueWait(Action finished)
        {
            StartCoroutine(continueWaitRoutine(finished));
        }
        private IEnumerator continueWaitRoutine(Action finished)
        {
            yield return CurrentWaiting.Wait();
            CurrentWaiting = null;

            if (finished == null)
                onFinished();
            else
                finished.Invoke();
        }
        public void cancelWait()
        {
            CurrentWaiting.Cancel();
        }

        protected void followPath(IEnumerable<Vector3> path, Action finished)
        {
            followPath(path, Info.Delay, finished);
        }
        protected void followPath(IEnumerable<Vector3> path, float delay, Action finished)
        {
            StartCoroutine(new WalkingPath(path.ToArray()).Walk(this, delay, finished ?? onFinished));
        }
        protected void continueFollow(Action finished)
        {
            StartCoroutine(CurrentWalking.WalkingPath.ContinueWalk(this, finished ?? onFinished));
        }

        protected void tryWalk(IBuilding target, float delay, Action planned = null, Action finished = null, Action canceled = null)
        {
            tryWalk(() => PathHelper.FindPath(_current, target, PathType, PathTag), delay, planned, finished, canceled);
        }
        protected void tryWalk(Vector2Int target, float delay, Action planned = null, Action finished = null, Action canceled = null)
        {
            tryWalk(() => PathHelper.FindPath(_current, target, PathType, PathTag), delay, planned, finished, canceled);
        }
        protected void tryWalk(Func<WalkingPath> pathGetter, float delay, Action planned = null, Action finished = null, Action canceled = null)
        {
            StartCoroutine(WalkingPath.TryWalk(this, delay, _current, pathGetter, planned: planned, finished: finished ?? onFinished, canceled: canceled, moved: onMoved));
        }

        protected void tryWalk(IBuilding target, Action planned = null, Action finished = null, Action canceled = null)
        {
            tryWalk(() => PathHelper.FindPath(_current, target, PathType, PathTag), planned, finished, canceled);
        }
        protected void tryWalk(Vector2Int target, Action planned = null, Action finished = null, Action canceled = null)
        {
            tryWalk(() => PathHelper.FindPath(_current, target, PathType, PathTag), planned, finished, canceled);
        }
        protected void tryWalk(Func<WalkingPath> pathGetter, Action planned = null, Action finished = null, Action canceled = null)
        {
            StartCoroutine(WalkingPath.TryWalk(this, Info.Delay, _current, pathGetter, planned: planned, finished: finished ?? onFinished, canceled: canceled, moved: onMoved));
        }

        protected internal virtual void onIsWalkingChanged(bool value)
        {
            IsWalkingChanged?.Invoke(value);
            Info.SetAnimationWalk(this, value);
        }
        protected internal virtual void onDirectionChanged(Vector3 direction)
        {
            DirectionChanged?.Invoke(direction);
            Info.SetAnimationDirection(this, direction);
            Dependencies.Get<IGridRotations>()?.SetRotation(Pivot, direction);
        }
        protected internal virtual void onItemStorageChanged(ItemStorage itemStorage)
        {
            Info.SetAnimationCarry(this, itemStorage.HasItems());
        }

        public void Hide()
        {
            Pivot.gameObject.SetActive(false);
            var collider = GetComponent<Collider2D>();
            if (collider)
                collider.enabled = false;
        }
        public void Show()
        {
            Pivot.gameObject.SetActive(true);
            var collider = GetComponent<Collider2D>();
            if (collider)
                collider.enabled = true;
        }

        public void Finish() => onFinished();

        public virtual string GetName() => Info.Name;
        public virtual string GetDescription() => Info.Descriptions.FirstOrDefault();
        public virtual string GetDebugText() => null;
        protected string getDescription(int index) => Info.Descriptions.ElementAtOrDefault(index);
        protected string getDescription(int index, params object[] parameters)
        {
            if (parameters == null)
                return getDescription(index);
            else
                return string.Format(getDescription(index), parameters);
        }

        #region Saving
        [Serializable]
        public class WalkerData
        {
            public string Id;
            public string HomeId;
            public Vector2Int StartPoint;
            public Vector2Int CurrentPoint;
            public Vector3 Position;
            public Vector3 PivotPosition;
            public Quaternion Rotation;
            public Quaternion PivotRotation;
            public ProcessState.ProcessData CurrentProcess;
            public RoamingState.RoamingData CurrentRoaming;
            public WalkingState.WalkingData CurrentWalking;
            public WalkingAgentState.WalkingAgentData CurrentWalkingAgent;
            public WaitingState.WaitingData CurrentWait;
            public float? HeightOverride;
        }

        public virtual string SaveData() => JsonUtility.ToJson(savewalkerData());
        public virtual void LoadData(string json) => loadWalkerData(JsonUtility.FromJson<WalkerData>(json));

        protected WalkerData savewalkerData()
        {
            return new WalkerData()
            {
                Id = Id.ToString(),
                HomeId = Home?.Instance.Id.ToString(),
                StartPoint = _start,
                CurrentPoint = _current,
                Position = transform.position,
                Rotation = transform.rotation,
                PivotPosition = Pivot.localPosition,
                PivotRotation = Pivot.localRotation,
                CurrentProcess = CurrentProcess?.GetData(),
                CurrentWalking = CurrentWalking?.GetData(),
                CurrentWalkingAgent = CurrentAgentWalking?.GetData(),
                CurrentRoaming = CurrentRoaming?.GetData(),
                CurrentWait = CurrentWaiting?.GetData(),
                HeightOverride = HeightOverride
            };
        }
        protected void loadWalkerData(WalkerData data)
        {
            _isLoaded = true;
            Id = new Guid(data.Id);
            if (!string.IsNullOrWhiteSpace(data.HomeId))
                Home = Dependencies.Get<IBuildingManager>().GetBuildingReference(new Guid(data.HomeId));
            _start = data.StartPoint;
            _current = data.CurrentPoint;
            transform.position = data.Position;
            transform.rotation = data.Rotation;
            Pivot.localPosition = data.PivotPosition;
            Pivot.localRotation = data.PivotRotation;
            CurrentProcess = ProcessState.FromData(data.CurrentProcess);
            CurrentRoaming = RoamingState.FromData(data.CurrentRoaming);
            CurrentWalking = WalkingState.FromData(data.CurrentWalking);
            CurrentAgentWalking = WalkingAgentState.FromData(data.CurrentWalkingAgent);
            CurrentWaiting = WaitingState.FromData(data.CurrentWait);
            HeightOverride = data.HeightOverride;
        }
        #endregion
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class WalkerEvent : UnityEvent<Walker> { }
}