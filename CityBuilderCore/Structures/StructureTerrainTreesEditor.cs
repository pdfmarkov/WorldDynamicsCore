#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CityBuilderCore
{
    [CustomEditor(typeof(StructureTerrainTrees))]
    public class StructureTerrainTreesEditor : Editor
    {
        private int _num;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();
            _num = EditorGUILayout.IntField(_num);
            if (GUILayout.Button("Add Random"))
            {
                var trees = (StructureTerrainTrees)target;
                var terrain = trees.TerrainModifier.GetComponent<Terrain>();

                var treeInstances = terrain.terrainData.treeInstances.ToList();

                for (int i = 0; i < _num; i++)
                {
                    var size = Random.Range(trees.MinHeight, trees.MaxHeight);
                    var color = 1f - Random.Range(0, trees.ColorVariation);

                    treeInstances.Add(new TreeInstance()
                    {
                        prototypeIndex = trees.Index,
                        position = new Vector3(Random.Range(0, 1f), 0f, Random.Range(0, 1f)),
                        heightScale = size,
                        widthScale = size,
                        color = new Color(color, color, color),
                        lightmapColor = Color.white
                    });
                }

                terrain.terrainData.SetTreeInstances(treeInstances.ToArray(), true);
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Clear"))
            {
                var trees = (StructureTerrainTrees)target;
                var terrain = trees.TerrainModifier.GetComponent<Terrain>();

                terrain.terrainData.SetTreeInstances(terrain.terrainData.treeInstances.Where(i => i.prototypeIndex != trees.Index).ToArray(), true);
            }
        }
    }
}
#endif