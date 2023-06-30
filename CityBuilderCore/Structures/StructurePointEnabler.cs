using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    public class StructurePointEnabler : MonoBehaviour
    {
        public string StructureKey;
        public Transform[] Points;
        public GameObject GameObject;

        private IGridPositions _gridPositions;

        private void Start()
        {
            _gridPositions = Dependencies.Get<IGridPositions>();
            Dependencies.Get<IStructureManager>().Changed += structuresChanged;
        }

        private void structuresChanged()
        {
            var structure= Dependencies.Get<IStructureManager>().GetStructure(StructureKey);
            if (structure == null)
                GameObject.SetActive(false);
            else
                GameObject.SetActive(Points.Select(p => _gridPositions.GetGridPosition(p.position)).All(p => structure.HasPoint(p)));
        }
    }
}
