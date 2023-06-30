using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// default implementation for <see cref="IBuilding"/>
    /// </summary>
    public class Building : MonoBehaviour, IBuilding, ISaveData
    {
        [Tooltip("contains meta info about the kind of building this is, for example size, name, building requirements")]
        public BuildingInfo Info;
        [Tooltip("transform that typically should be located at the center of the building and contain all of its visuals")]
        public Transform Pivot;

        public virtual Vector2Int RawSize => Info.Size;
        public virtual Vector2Int Point => _point ?? Rotation.UnrotateOrigin(Dependencies.Get<IGridPositions>().GetGridPosition(transform.position), RawSize);
        public virtual Vector2Int Size => _size ?? Rotation.RotateSize(RawSize);
        public BuildingRotation Rotation => _rotation ?? BuildingRotation.Create(transform.localRotation);
        public Vector2Int AccessPoint => _accessPoint ?? Rotation.RotateBuildingPoint(Point, Info.AccessPoint, RawSize);

        public Vector3 WorldCenter => _worldCenter ?? Dependencies.Get<IGridPositions>().GetWorldPosition(Point) + (Dependencies.Get<IMap>().IsXY ? new Vector3(Size.x / 2f, Size.y / 2f, 0) : new Vector3(Size.x / 2f, 0f, Size.y / 2f));

        public virtual Vector3 PrimaryPosition => WorldCenter;
        public virtual Vector3 SecondaryPosition => WorldCenter;

        public StructureReference StructureReference { get; set; }
        public BuildingReference BuildingReference { get; set; }

        public string Key => Info.Key;
        public bool IsDestructible => Info.IsDestructible;
        public virtual bool IsDecorator => false;
        public virtual bool IsWalkable => Info.IsWalkable;

        public Guid Id { get; set; } = Guid.NewGuid();
        public int Index { get; set; }

        BuildingInfo IBuilding.Info => Info;
        Transform IBuilding.Pivot => Pivot;
        int IStructure.Level => Info?.Level?.Value ?? 0;

        public Transform Root => transform;

        public float Efficiency
        {
            get
            {
                if (Dependencies.Contains<IGameSettings>() && !Dependencies.Get<IGameSettings>().HasEfficiency)
                    return 1f;

                float efficiency = 1f;
                foreach (var factor in GetBuildingParts<IEfficiencyFactor>().Select(e => e.Factor))
                {
                    efficiency *= factor;
                }

                return efficiency;
            }
        }
        public bool IsWorking
        {
            get
            {
                if (Dependencies.Contains<IGameSettings>() && !Dependencies.Get<IGameSettings>().HasEfficiency)
                    return true;

                return GetBuildingParts<IEfficiencyFactor>().All(e => e.IsWorking);
            }
        }

        public event Action<PointsChanged<IStructure>> PointsChanged { add { } remove { } }
        public event Action<IBuilding, IBuilding> Replacing;

        protected int _prefabIndex;
        protected Vector2Int? _point;
        protected Vector2Int? _size;
        protected BuildingRotation _rotation;
        protected Vector2Int? _accessPoint;
        protected Vector3? _worldCenter;

        protected IBuildingComponent[] _components;
        public IBuildingComponent[] Components => _components ?? GetComponents<IBuildingComponent>();

        protected List<BuildingAddon> _addons = new List<BuildingAddon>();
        public IReadOnlyCollection<BuildingAddon> Addons => _addons;

        private bool _isReplaced = false;

        protected virtual void Awake()
        {
            _components = GetComponents<IBuildingComponent>();
            _components.ForEach(c => c.Building = this);
        }

        protected virtual void Start()
        {
            _point = Point;
            _size = Size;
            _rotation = Rotation;
            _accessPoint = AccessPoint;
            _worldCenter = WorldCenter;

            if (StructureReference == null)
            {
                Setup();
                Initialize();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            try
            {
                var text = string.Join(Environment.NewLine, Components.Select(c => c.GetDebugText()).Where(t => !string.IsNullOrWhiteSpace(t)));
                if (string.IsNullOrWhiteSpace(text))
                    return;

                UnityEditor.Handles.Label(WorldCenter, text);
            }
            catch
            {
                //dont care
            }
        }
#endif

        public virtual void Initialize()
        {
            StructureReference = new StructureReference(this);
            BuildingReference = new BuildingReference(this);

            Dependencies.Get<IStructureManager>().RegisterStructure(this);
            Dependencies.Get<IBuildingManager>().RegisterBuilding(this);

            var height = Dependencies.GetOptional<IGridHeights>();
            if (height != null)
                height.ApplyHeight(Pivot);

            _components.ForEach(c => c.InitializeComponent());
        }
        public virtual void Setup()
        {
            _components.ForEach(c => c.SetupComponent());
        }
        public T Replace<T>(T prefab) where T : MonoBehaviour, IBuilding
        {
            if (_isReplaced)
                return null;//make sure a building is not replaced again before being destroyed
            _isReplaced = true;

            var replacement = (T)(object)Instantiate((UnityEngine.Object)(object)prefab, transform.position, transform.rotation, transform.parent);

            replacement.Index = prefab.Info.GetPrefabIndex(prefab);

            onReplacing(replacement);

            StructureReference.Replace(replacement);
            BuildingReference.Replace(replacement);

            if (Info?.Level?.Value != replacement.Level)
            {
                var structureManager = Dependencies.Get<IStructureManager>();

                structureManager.DeregisterStructure(this);
                structureManager.RegisterStructure(replacement);
            }

            Components.ForEach(c => c.OnReplacing(replacement));
            Addons.ForEach(a => a.OnReplacing(replacement.transform, replacement));

            Replacing?.Invoke(this, replacement);

            Destroy(gameObject);

            return replacement;
        }
        public virtual void Terminate()
        {
            _components.ForEach(c => c.TerminateComponent());

            Dependencies.Get<IStructureManager>().DeregisterStructure(this);
            Dependencies.Get<IBuildingManager>().DeregisterBuilding(this);

            Destroy(gameObject);
        }

        protected virtual void onReplacing(IBuilding replacement)
        {
        }

        public bool HasBuildingPart<T>() => GetBuildingParts<T>().Any();
        public IEnumerable<T> GetBuildingParts<T>()
        {
            foreach (var component in Components.OfType<T>())
            {
                yield return component;
            }
            foreach (var addon in Addons.OfType<T>())
            {
                yield return addon;
            }
        }

        public bool HasBuildingComponent<T>()
            where T : IBuildingComponent
        {
            return Components.OfType<T>().Any();
        }
        public T GetBuildingComponent<T>()
            where T : class, IBuildingComponent
        {
            return Components.OfType<T>().FirstOrDefault();
        }
        public IEnumerable<T> GetBuildingComponents<T>()
            where T : IBuildingComponent
        {
            return Components.OfType<T>();
        }

        public bool HasBuildingAddon<T>()
            where T : BuildingAddon
        {
            return Addons.OfType<T>().Any();
        }
        public T GetBuildingAddon<T>()
            where T : BuildingAddon
        {
            return Addons.OfType<T>().FirstOrDefault();
        }
        public IEnumerable<T> GetBuildingAddons<T>()
            where T : BuildingAddon
        {
            return Addons.OfType<T>();
        }

        public T AddAddon<T>(T prefab) where T : BuildingAddon
        {
            var addon = Instantiate(prefab, transform);

            _addons.Add(addon);

            addon.Building = this;
            addon.InitializeAddon();

            return addon;
        }
        public T GetAddon<T>(string key) where T : BuildingAddon
        {
            return _addons.OfType<T>().FirstOrDefault(a => key == null || a.Key == key);
        }
        public void RemoveAddon(BuildingAddon addon)
        {
            addon.TerminateAddon();

            _addons.Remove(addon);
        }
        public bool RemoveAddon(string key)
        {
            var addon = _addons.FirstOrDefault(a => a.Key == key);
            if (addon == null)
                return false;

            RemoveAddon(addon);
            return true;
        }

        public virtual IEnumerable<Vector2Int> GetAccessPoints(PathType type, object tag)
        {
            if (Info.AccessType != BuildingAccessType.Any)
            {
                if (PathHelper.CheckPoint(AccessPoint, type, tag))
                    yield return AccessPoint;
            }

            if (Info.AccessType != BuildingAccessType.Exclusive)
            {
                foreach (var point in getSpawnPoints())
                {
                    if (PathHelper.CheckPoint(point, type, tag))
                        yield return point;
                }
            }
        }
        public virtual Vector2Int? GetAccessPoint(PathType type, object tag)
        {
            var points = GetAccessPoints(type, tag);
            if (points.Any())
                return points.First();
            else
                return null;
        }

        public bool HasAccessPoint(PathType type, object tag = null) => GetAccessPoints(type, tag).Any();

        public IEnumerable<Vector2Int> GetPoints()
        {
            return PositionHelper.GetStructurePositions(Point, Size);
        }

        public bool HasPoint(Vector2Int position) => PositionHelper.GetStructurePositions(Point, Size).Contains(position);

        protected virtual IEnumerable<Vector2Int> getSpawnPoints()
        {
            if (Info.RoadRequirements != null && Info.RoadRequirements.Length > 0)
            {
                foreach (var requirement in Info.RoadRequirements)
                {
                    yield return Rotation.RotateBuildingPoint(Point, requirement.Point, RawSize);
                }
            }
            else
            {
                foreach (var point in PositionHelper.GetAdjacent(Point, Size))
                {
                    yield return point;
                }
            }
        }

        public virtual void Add(IEnumerable<Vector2Int> points) { }
        public virtual void Remove(IEnumerable<Vector2Int> points)
        {
            Terminate();
        }

        public virtual string GetName() => Info.Name;
        public virtual string GetDescription() => Info.Description;

        #region Saving
        [Serializable]
        public class BuildingData
        {
            public BuildingComponentMetaData[] Components;
            public BuildingAddonMetaData[] Addons;
        }
        [Serializable]
        public class BuildingComponentMetaData
        {
            public string Key;
            public string Data;
        }
        [Serializable]
        public class BuildingAddonMetaData
        {
            public string Key;
            public string Data;
        }
        public virtual string SaveData()
        {
            return JsonUtility.ToJson(new BuildingData()
            {
                Components = _components.Select(c =>
                {
                    var data = c.SaveData();
                    if (string.IsNullOrWhiteSpace(data))
                        return null;

                    return new BuildingComponentMetaData()
                    {
                        Key = c.Key,
                        Data = data
                    };
                }).Where(d => d != null).ToArray(),
                Addons = _addons.Select(a =>
                {
                    return new BuildingAddonMetaData()
                    {
                        Key = a.Key,
                        Data = a.SaveData()
                    };
                }).Where(d => d != null).ToArray()
            });
        }
        public virtual void LoadData(string json)
        {
            var data = JsonUtility.FromJson<BuildingData>(json);

            foreach (var componentMetaData in data.Components)
            {
                var component = _components.FirstOrDefault(c => c.Key == componentMetaData.Key);
                if (component == null)
                    continue;

                component.LoadData(componentMetaData.Data);
            }

            var addons = Dependencies.Get<IKeyedSet<BuildingAddon>>();

            foreach (var addonMetaData in data.Addons)
            {
                var addon = AddAddon(addons.GetObject(addonMetaData.Key));
                if (addon == null)
                    continue;
                if (string.IsNullOrWhiteSpace(addonMetaData.Data))
                    continue;

                addon.LoadData(addonMetaData.Data);
            }
        }
        #endregion
    }
}