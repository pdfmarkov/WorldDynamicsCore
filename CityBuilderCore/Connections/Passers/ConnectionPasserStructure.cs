using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// will pass a connection at the points of a structure in the same gameobject or its parent
    /// </summary>
    public class ConnectionPasserStructure : ConnectionPasserBase
    {
        private IStructure _structure;

        private void Awake()
        {
            _structure = GetComponent<IStructure>() ?? GetComponentInParent<IStructure>();
            _structure.PointsChanged += structurePointsChanged;
        }

        public override IEnumerable<Vector2Int> GetPoints() => _structure.GetPoints();

        private void structurePointsChanged(PointsChanged<IStructure> change) => onPointsChanged(change.RemovedPoints, change.AddedPoints);
    }
}
