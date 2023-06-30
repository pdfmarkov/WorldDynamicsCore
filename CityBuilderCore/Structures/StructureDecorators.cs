using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// structure made up of different decorators, used for random map objects that will be removed when something is built in their place<br/>
    /// the prefab that will be used for when the game is loaded is determined by the gameobjects name so this should at least start with the prefab name
    /// </summary>
    public class StructureDecorators : KeyedBehaviour, IStructure
    {
        [Tooltip("determines which other structures can reside in the same points")]
        public StructureLevelMask Level;
        [Tooltip("all the prefabs that can be used to replace the decorators after loading, gameobject names have to start with the prefab name for this to work")]
        public GameObject[] Prefabs;

        bool IStructure.IsDestructible => true;
        bool IStructure.IsDecorator => true;
        bool IStructure.IsWalkable => true;
        int IStructure.Level => Level.Value;

        public StructureReference StructureReference { get; set; }

        public event Action<PointsChanged<IStructure>> PointsChanged;

        private Dictionary<Vector2Int, GameObject> _objects;

        private void Start()
        {
            if (_objects == null)
                loadObjects();

            StructureReference = new StructureReference(this);
            Dependencies.Get<IStructureManager>().RegisterStructure(this);
            PointsChanged?.Invoke(new PointsChanged<IStructure>(this, Enumerable.Empty<Vector2Int>(), GetPoints()));
        }

        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded)
                Dependencies.Get<IStructureManager>().DeregisterStructure(this);
        }

        public IEnumerable<Vector2Int> GetChildPoints(IGridPositions positions)
        {
            foreach (Transform child in transform)
            {
                yield return positions.GetGridPosition(child.position);
            }
        }

        public IEnumerable<Vector2Int> GetPoints() => _objects.Keys;

        public bool HasPoint(Vector2Int position) => _objects.ContainsKey(position);

        public void Add(IEnumerable<Vector2Int> points)
        {
            throw new NotImplementedException();
        }
        public void Remove(IEnumerable<Vector2Int> points)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (var point in points)
            {
                if (!_objects.ContainsKey(point))
                    continue;

                Destroy(_objects[point]);
                _objects.Remove(point);
            }

            PointsChanged?.Invoke(new PointsChanged<IStructure>(this, points, Enumerable.Empty<Vector2Int>()));
        }

        public void Clear()
        {
            _objects.ForEach(o => Destroy(o.Value));
            _objects.Clear();
        }

        public string GetName() => string.Empty;

        private void loadObjects()
        {
            _objects = new Dictionary<Vector2Int, GameObject>();
            var positions = Dependencies.Get<IGridPositions>();
            foreach (Transform child in transform)
            {
                var position = positions.GetGridPosition(child.position);

                _objects.Add(position, child.gameObject);
            }
        }

        #region Saving
        [Serializable]
        public class StructureDecoratorsData
        {
            public string Key;
            public StructureDecoratorData[] Decorators;

        }
        [Serializable]
        public class StructureDecoratorData
        {
            public string Prefab;
            public Vector2Int[] Points;
            public float[] Rotations;
            public float[] Scales;
        }

        public StructureDecoratorsData SaveData()
        {
            var gridRotations = Dependencies.Get<IGridRotations>();
            var decorators = new List<StructureDecoratorData>();

            foreach (var prefab in Prefabs)
            {
                var points = new List<Vector2Int>();
                var rotations = new List<float>();
                var scales = new List<float>();

                foreach (var o in _objects)
                {
                    if (o.Value.name.StartsWith(prefab.name))
                    {
                        points.Add(o.Key);
                        rotations.Add(gridRotations.GetRotation(o.Value.transform));
                        scales.Add(o.Value.transform.localScale.x / prefab.transform.localScale.x);
                    }
                }

                if (points.Count > 0)
                {
                    decorators.Add(new StructureDecoratorData()
                    {
                        Prefab = prefab.name,
                        Points = points.ToArray(),
                        Rotations = rotations.ToArray(),
                        Scales = scales.ToArray()
                    });
                }
            }

            return new StructureDecoratorsData() { Key = Key, Decorators = decorators.ToArray() };
        }
        public void LoadData(StructureDecoratorsData data)
        {
            if (_objects == null)
                loadObjects();

            var oldPoints = _objects.Keys.ToList();
            var newPoints = new List<Vector2Int>();

            var positions = Dependencies.Get<IGridPositions>();
            var rotations = Dependencies.Get<IGridRotations>();

            foreach (var decorator in data.Decorators)
            {
                var prefab = Prefabs.FirstOrDefault(p => p.name.Equals(decorator.Prefab));
                if (prefab == null)
                {
                    Debug.LogError($"Decorator {name} could not find prefab {decorator.Prefab}");
                    continue;
                }

                for (int i = 0; i < decorator.Points.Length; i++)
                {
                    var point = decorator.Points[i];
                    var rotation = decorator.Rotations[i];
                    var scale = decorator.Scales[i];

                    if (oldPoints.Contains(point))
                    {
                        oldPoints.Remove(point);
                    }
                    else
                    {
                        newPoints.Add(point);

                        var instance = Instantiate(prefab, transform);
                        instance.transform.position = positions.GetWorldCenterPosition(point);
                        instance.transform.localScale = prefab.transform.localScale * scale;
                        rotations.SetRotation(instance.transform, rotation);

                        _objects.Add(point, instance);
                    }
                }
            }

            foreach (var point in oldPoints)
            {
                Destroy(_objects[point]);
                _objects.Remove(point);
            }

            PointsChanged?.Invoke(new PointsChanged<IStructure>(this, oldPoints, newPoints));
        }
        #endregion
    }
}