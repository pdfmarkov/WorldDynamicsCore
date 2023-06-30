using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CityBuilderCore
{
    /// <summary>
    /// default implementation for <see cref="IHighlightManager"/><br/>
    /// uses tiles that are assigned in inspector
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class DefaultHighlightManager : MonoBehaviour, IHighlightManager
    {
        [Tooltip("tile that is used when indicating that a point on the map is valid(for example for building on)")]
        public TileBase ValidTile;
        [Tooltip("tile that is used when indicating that a point on the map is invalid(for example when building is blocked)")]
        public TileBase InvalidTile;
        [Tooltip("tile that is used when indicating that a point on the map is somehow special(for example for the building access point)")]
        public TileBase InfoTile;

        private Tilemap _tilemap;

        protected virtual void Awake()
        {
            Dependencies.Register<IHighlightManager>(this);

            _tilemap = GetComponent<Tilemap>();
        }

        public void Highlight(IEnumerable<Vector2Int> points, bool valid) => Highlight(points, valid ? ValidTile : InvalidTile);
        public void Highlight(IEnumerable<Vector2Int> points, HighlightType type) => Highlight(points, getTile(type));
        public void Highlight(IEnumerable<Vector2Int> points, TileBase tile)
        {
            foreach (var position in points)
            {
                _tilemap.SetTile((Vector3Int)position, tile);
            }
        }

        public void Highlight(Vector2Int point, bool isValid) => Highlight(point, isValid ? ValidTile : InvalidTile);
        public void Highlight(Vector2Int point, HighlightType type) => Highlight(point, getTile(type));
        public void Highlight(Vector2Int point, TileBase tile)
        {
            _tilemap.SetTile((Vector3Int)point, tile);
        }

        private TileBase getTile(HighlightType type)
        {
            switch (type)
            {
                case HighlightType.Valid:
                    return ValidTile;
                case HighlightType.Invalid:
                    return InvalidTile;
                case HighlightType.Info:
                    return InfoTile;
                default:
                    return null;
            }
        }

        public void Clear()
        {
            _tilemap.ClearAllTiles();
        }
    }
}