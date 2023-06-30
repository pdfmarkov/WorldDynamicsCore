using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CityBuilderCore
{
    /// <summary>
    /// can be used to update the navmesh at runtime using known components<br/>
    /// source creation taken from https://github.com/Unity-Technologies/NavMeshComponents
    /// </summary>
    public class NavMeshUpdater : MonoBehaviour
    {
        public Bounds Bounds;

        public Terrain[] Terrains;
        public MeshFilter[] Meshes;
        public MeshFilter[] MeshesNotWalkable;

        public float AgentRadius = 0.5f;
        public float AgentHeight = 2f;
        public float MaxSlope = 45;
        public float StepHeight = 0.5f;

        private NavMeshData _navMeshData;
        private NavMeshDataInstance _navMeshInstance;
        private List<NavMeshBuildSource> _sources;

        public void UpdateNavMesh()
        {
            if (_sources == null)
            {
                _navMeshData = new NavMeshData();
                _navMeshInstance = NavMesh.AddNavMeshData(_navMeshData);

                _sources = new List<NavMeshBuildSource>();
            }
            else
            {
                _sources.Clear();
            }

            for (var i = 0; i < Meshes.Length; ++i)
            {
                var mf = Meshes[i];
                if (mf == null) continue;

                var m = mf.sharedMesh;
                if (m == null) continue;

                var s = new NavMeshBuildSource();
                s.shape = NavMeshBuildSourceShape.Mesh;
                s.sourceObject = m;
                s.transform = mf.transform.localToWorldMatrix;
                s.area = 0;
                _sources.Add(s);
            }

            for (var i = 0; i < MeshesNotWalkable.Length; ++i)
            {
                var mf = MeshesNotWalkable[i];
                if (mf == null) continue;

                var m = mf.sharedMesh;
                if (m == null) continue;

                var s = new NavMeshBuildSource();
                s.shape = NavMeshBuildSourceShape.Mesh;
                s.sourceObject = m;
                s.transform = mf.transform.localToWorldMatrix;
                s.area = 1;
                _sources.Add(s);
            }

            for (var i = 0; i < Terrains.Length; ++i)
            {
                var t = Terrains[i];
                if (t == null) continue;

                var s = new NavMeshBuildSource();
                s.shape = NavMeshBuildSourceShape.Terrain;
                s.sourceObject = t.terrainData;
                // Terrain system only supports translation - so we pass translation only to back-end
                s.transform = Matrix4x4.TRS(t.transform.position, Quaternion.identity, Vector3.one);
                s.area = 0;
                _sources.Add(s);
            }

            var settings = new NavMeshBuildSettings()
            {
                agentRadius = AgentRadius,
                agentHeight = AgentHeight,
                agentSlope = MaxSlope,
                agentClimb = StepHeight,
                minRegionArea = 2,
                tileSize = 256,
                voxelSize = 0.166667f
            };

            NavMeshBuilder.UpdateNavMeshData(_navMeshData, settings, _sources, Bounds);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }
    }
}
