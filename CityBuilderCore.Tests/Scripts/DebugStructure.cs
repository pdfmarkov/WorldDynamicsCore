using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore.Tests
{
    public class DebugStructure : KeyedBehaviour, IStructure
    {
        public StructureReference StructureReference { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Transform Root => transform;

        public bool IsDestructible => false;
        public bool IsDecorator => false;
        public bool IsWalkable => false;
        public int Level => 0;

        public event Action<PointsChanged<IStructure>> PointsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<Vector2Int> GetPoints() => new Vector2Int[] { Dependencies.Get<IGridPositions>().GetGridPosition(transform.position) };

        public bool HasPoint(Vector2Int position) => GetPoints().Contains(position);

        public void Add(IEnumerable<Vector2Int> points) { }
        public void Remove(IEnumerable<Vector2Int> positions) { }

        public string GetName() => "DEBUG";

    }
}
