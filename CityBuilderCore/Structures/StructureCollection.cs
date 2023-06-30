﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// structure made up of a collection of identical gameobjects<br/>
    /// if the members of the collection are <see cref="ISaveData"/> that data will also be stored
    /// </summary>
    public class StructureCollection : KeyedBehaviour, IStructure
    {
        [Tooltip("name of the structure in the UI")]
        public string Name;

        [Tooltip("whether the structure can be removed by the player")]
        public bool IsDestructible = true;
        [Tooltip("whether the structure is automatically removed when something is built on top of it")]
        public bool IsDecorator = false;
        [Tooltip("whether walkers can pass the points of this structure")]
        public bool IsWalkable = false;

        [Tooltip("determines which other structures can reside in the same points")]
        public StructureLevelMask Level;

        [Tooltip("the prefab that is instantiated when points are added or when the game is loaded, origin should be the corner of its origin point")]
        public GameObject Prefab;
        [Tooltip("the size that one object in this collection occupies")]
        public Vector2Int ObjectSize = Vector2Int.one;

        bool IStructure.IsDestructible => IsDestructible;
        bool IStructure.IsDecorator => IsDecorator;
        bool IStructure.IsWalkable => IsWalkable;
        int IStructure.Level => Level.Value;

        public StructureReference StructureReference { get; set; }

        public bool IsReplica { get; set; }

        public Transform Root => transform;

        public event Action<PointsChanged<IStructure>> PointsChanged;

        private Dictionary<Vector2Int, GameObject> _objects = new Dictionary<Vector2Int, GameObject>();

        private void Start()
        {
            var positions = Dependencies.Get<IGridPositions>();
            foreach (Transform child in transform)
            {
                var position = positions.GetGridPosition(child.position);

                for (int x = 0; x < ObjectSize.x; x++)
                {
                    for (int y = 0; y < ObjectSize.y; y++)
                    {
                        _objects.Add(position + new Vector2Int(x, y), child.gameObject);
                    }
                }
            }

            StructureReference = new StructureReference(this);

            if (!IsReplica)
                Dependencies.Get<IStructureManager>().RegisterStructure(this);

            PointsChanged?.Invoke(new PointsChanged<IStructure>(this, Enumerable.Empty<Vector2Int>(), GetPoints()));
        }

        public IEnumerable<Vector2Int> GetChildPoints(IGridPositions positions)
        {
            foreach (Transform child in transform)
            {
                for (int x = 0; x < ObjectSize.x; x++)
                {
                    for (int y = 0; y < ObjectSize.y; y++)
                    {
                        yield return positions.GetGridPosition(child.position) + new Vector2Int(x, y);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded)
                Dependencies.Get<IStructureManager>().DeregisterStructure(this);
        }

        public IEnumerable<Vector2Int> GetPoints() => _objects.Keys;

        public bool HasPoint(Vector2Int position) => _objects.ContainsKey(position);

        public void Add(Vector2Int point) => Add(new Vector2Int[] { point });
        public void Add(IEnumerable<Vector2Int> points)
        {
            foreach (var point in points)
            {
                if (_objects.ContainsKey(point))
                    continue;

                var instance = Instantiate(Prefab, transform);
                instance.transform.position = Dependencies.Get<IGridPositions>().GetWorldPosition(point);
                _objects.Add(point, instance);
            }

            PointsChanged?.Invoke(new PointsChanged<IStructure>(this, Enumerable.Empty<Vector2Int>(), points));
        }
        public void Remove(Vector2Int point) => Remove(new Vector2Int[] { point });
        public void Remove(IEnumerable<Vector2Int> points)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (var point in points)
            {
                if (_objects.ContainsKey(point) && !children.Contains(_objects[point]))
                    children.Add(_objects[point]);
            }

            foreach (var child in children)
            {
                var position = Dependencies.Get<IGridPositions>().GetGridPosition(child.transform.position);

                for (int x = 0; x < ObjectSize.x; x++)
                {
                    for (int y = 0; y < ObjectSize.y; y++)
                    {
                        _objects.Remove(position + new Vector2Int(x, y));
                    }
                }

                Destroy(child);
            }

            PointsChanged?.Invoke(new PointsChanged<IStructure>(this, points, Enumerable.Empty<Vector2Int>()));
        }

        public void Clear()
        {
            _objects.ForEach(o => Destroy(o.Value));
            _objects.Clear();
        }

        public string GetName() => Name;

        #region Saving
        [Serializable]
        public class StructureCollectionData
        {
            public string Key;
            public Vector2Int[] Positions;
            public string[] InstanceData;
        }

        public StructureCollectionData SaveData()
        {
            var data = new StructureCollectionData();

            data.Key = Key;
            data.Positions = _objects.Keys.ToArray();

            if (Prefab.GetComponent<ISaveData>() != null)
                data.InstanceData = _objects.Values.Select(o => o.GetComponent<ISaveData>().SaveData()).ToArray();

            return data;
        }
        public void LoadData(StructureCollectionData data)
        {
            var oldPoints = _objects.Keys.ToList();

            Clear();

            foreach (var position in data.Positions)
            {
                var instance = Instantiate(Prefab, transform);
                instance.transform.position = Dependencies.Get<IGridPositions>().GetWorldPosition(position);
                _objects.Add(position, instance);
            }

            if (data.InstanceData != null && data.InstanceData.Length == _objects.Count)
            {
                for (int i = 0; i < data.InstanceData.Length; i++)
                {
                    _objects.ElementAt(i).Value.GetComponent<ISaveData>().LoadData(data.InstanceData[i]);
                }
            }

            PointsChanged?.Invoke(new PointsChanged<IStructure>(this, oldPoints, GetPoints()));
        }
        #endregion
    }
}