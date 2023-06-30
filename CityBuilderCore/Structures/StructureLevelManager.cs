using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// helper used by <see cref="DefaultStructureManager"/> to store the structure references for one structure level
    /// </summary>
    public class StructureLevelManager
    {
        private readonly Dictionary<Vector2Int, StructureReference> _structurePositions = new Dictionary<Vector2Int, StructureReference>();

        public void AddStructure(IStructure structure)
        {
            foreach (var structurePosition in structure.GetPoints())
            {
                _structurePositions.Add(structurePosition, structure.StructureReference);
            }
        }
        public void RemoveStructure(IStructure structure)
        {
            foreach (var structurePosition in structure.GetPoints())
            {
                if (_structurePositions.ContainsKey(structurePosition) && _structurePositions[structurePosition] == structure.StructureReference)
                    _structurePositions.Remove(structurePosition);
            }
        }

        public IEnumerable<IStructure> GetStructures() => _structurePositions.Values.Distinct().Select(r => r.Instance);
        public IStructure GetStructure(Vector2Int position)
        {
            if (_structurePositions.ContainsKey(position))
                return _structurePositions[position].Instance;
            return null;
        }
    }
}
