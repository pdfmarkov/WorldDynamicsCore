using UnityEngine;

namespace CityBuilderCore
{
    [RequireComponent(typeof(Camera))]
    public class TerrainRenderSimplifier : MonoBehaviour
    {
        public Terrain Terrain;

        private float _defaultBillboardDistance;
        private float _defaultDetailDistance;

        private void Awake()
        {
            _defaultBillboardDistance = Terrain.treeBillboardDistance;
            _defaultDetailDistance = Terrain.detailObjectDistance;
        }

        private void OnPreCull()
        {
            Terrain.treeBillboardDistance = 0f;
            Terrain.detailObjectDistance = 0f;
        }
        void OnPostRender()
        {
            Terrain.treeBillboardDistance = _defaultBillboardDistance;
            Terrain.detailObjectDistance = _defaultDetailDistance;
        }
    }
}
