using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// optional component that keeps rotation between different builders
    /// </summary>
    public class BuildingRotationKeeper : MonoBehaviour
    {
        public int InitialRotation;

        public BuildingRotation Rotation { get; set; }

        private void Awake()
        {
            Dependencies.Register(this);
        }

        private void Start()
        {
            Rotation = BuildingRotation.Create(InitialRotation);
        }
    }
}
