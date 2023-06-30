using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CityBuilderCore
{
    public class ObjectGenerator : MonoBehaviour
    {
        [System.Serializable]
        public class TilemapTile
        {
            public Tilemap Tilemap;
            public Tile[] Tiles;

            public bool Check(Vector2Int point)
            {
                if (Tiles.Length > 0)
                {
                    return Tiles.Contains(Tilemap.GetTile((Vector3Int)point));

                }
                else
                {
                    return Tilemap.HasTile((Vector3Int)point);
                }
            }
        }

        [System.Serializable]
        public class GeneratorObject
        {
            public float Threshold;
            public GameObject Prefab;
        }

        public enum GeneratorMethod { Rand, Perlin }

        [Header("Conbstraints")]
        public TilemapTile[] TileRequirements;
        public TilemapTile[] TileBlockers;
        public StructureCollection[] StructureBlockers;
        public StructureDecorators[] DecoratorBlockers;
        [Header("Method")]
        public GeneratorMethod Method;
        public Vector2 NoiseScale;
        public Vector2 NoiseOffset;
        public int RandomSeed;
        [Header("Output")]
        public GeneratorObject[] Objects;
        public bool Rotate;
        public bool RotateStepped;
        public bool Scale;
        public float ScaleMinimum = 0.8f;
        public float ScaleMaximum = 1.2f;

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        public void Generate()
        {
            Clear();

            Random.InitState(RandomSeed);

            var map = FindObjectsOfType<MonoBehaviour>().OfType<IMap>().FirstOrDefault();
            var gridPositions = FindObjectsOfType<MonoBehaviour>().OfType<IGridPositions>().FirstOrDefault();
            var gridRotations = FindObjectsOfType<MonoBehaviour>().OfType<IGridRotations>().FirstOrDefault();
            if (map == null || gridPositions == null || gridRotations == null)
                return;

            var structureBlocked = StructureBlockers.SelectMany(b => b.GetChildPoints(gridPositions)).ToList();
            var decoratorBlocked = DecoratorBlockers.SelectMany(b => b.GetChildPoints(gridPositions)).ToList();

            List<Vector2Int> points = new List<Vector2Int>();
            for (int x = 0; x < map.Size.x; x++)
            {
                for (int y = 0; y < map.Size.y; y++)
                {
                    var point = new Vector2Int(x, y);
                    if (!TileRequirements.All(t => t.Check(point)))
                        continue;
                    if (TileBlockers.Any(t => t.Check(point)))
                        continue;

                    if (structureBlocked.Contains(point))
                        continue;
                    if (decoratorBlocked.Contains(point))
                        continue;

                    points.Add(point);
                }
            }

            Random.InitState(RandomSeed);

            foreach (var point in points)
            {
                float value;
                switch (Method)
                {
                    default:
                    case GeneratorMethod.Rand:
                        value = Random.Range(0f, 1f);
                        break;
                    case GeneratorMethod.Perlin:
                        value = Mathf.PerlinNoise(point.x * NoiseScale.x + NoiseOffset.x, point.y * NoiseScale.y + NoiseOffset.y);
                        break;
                }
                GeneratorObject generatorObject = null;
                foreach (var o in Objects)
                {
                    if (o.Threshold > value)
                        break;
                    generatorObject = o;
                }

                if (generatorObject != null)
                {
                    var instance = Instantiate(generatorObject.Prefab, gridPositions.GetWorldCenterPosition(point), Quaternion.identity, transform);

                    if (Rotate)
                    {
                        float rotation;

                        if (RotateStepped)
                            rotation = Random.Range(0, 3) * 90;
                        else
                            rotation = Random.Range(0f, 360f);

                        gridRotations.SetRotation(instance.transform, rotation);
                    }

                    if (Scale)
                    {
                        instance.transform.localScale *= Random.Range(ScaleMinimum, ScaleMaximum);
                    }
                }
            }
        }
    }
}
