using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// tool for adding points to a <see cref="StructureCollection"/> or <see cref="StructureTiles"/>
    /// </summary>
    public class StructureBuilder : PointerToolBase
    {
        [Tooltip("the collection that will have points added by the builder(set either Collection or Tiles)")]
        public StructureCollection Collection;
        [Tooltip("the tiles that will have points added by the builder(set either Collection or Tiles)")]
        public StructureTiles Tiles;
        [Tooltip("item cost per point that will be added")]
        public ItemQuantity[] Cost;

        public override string TooltipName
        {
            get
            {
                var name = string.Empty;
                if (Collection)
                    name = Collection.Name;
                else if (Tiles)
                    name = Tiles.Name;

                if (Cost != null && Cost.Length > 0)
                    name += $"({Cost.ToDisplayString()})";

                return name;
            }
        }

        private List<ItemQuantity> _costs = new List<ItemQuantity>();
        private IGlobalStorage _globalStorage;
        private IHighlightManager _highlighting;

        protected override void Start()
        {
            base.Start();

            _globalStorage = Dependencies.Get<IGlobalStorage>();
            _highlighting = Dependencies.Get<IHighlightManager>();
        }

        public override int GetCost(Item item)
        {
            return _costs.FirstOrDefault(c => c.Item == item)?.Quantity ?? 0;
        }

        protected override void updatePointer(Vector2Int mousePoint, Vector2Int dragStart, bool isDown, bool isApply)
        {
            _highlighting.Clear();

            List<Vector2Int> validPositions = new List<Vector2Int>();
            List<Vector2Int> invalidPositions = new List<Vector2Int>();

            IEnumerable<Vector2Int> positions;

            if (isDown)
                positions = PositionHelper.GetRoadPositions(dragStart, mousePoint);
            else if (IsTouchActivated)
                positions = new Vector2Int[] { };
            else
                positions = new Vector2Int[] { mousePoint };

            foreach (var position in positions)
            {
                if (Dependencies.Get<IStructureManager>().CheckAvailability(position, Collection ? Collection.Level.Value : Tiles.Level.Value) && Dependencies.Get<IMap>().IsInside(position))
                {
                    validPositions.Add(position);
                }
                else
                {
                    invalidPositions.Add(position);
                }
            }

            bool hasCost = true;
            _costs.Clear();
            foreach (var items in Cost)
            {
                _costs.Add(new ItemQuantity(items.Item, items.Quantity * validPositions.Count));

                if (!_globalStorage.Items.HasItemsRemaining(items.Item, items.Quantity * validPositions.Count))
                {
                    hasCost = false;
                }
            }

            if (!hasCost)
            {
                invalidPositions.AddRange(validPositions);
                validPositions.Clear();
            }

            _highlighting.Clear();
            _highlighting.Highlight(validPositions, true);
            _highlighting.Highlight(invalidPositions, false);

            if (isApply)
            {
                if (validPositions.Any())
                    onApplied();

                foreach (var items in Cost)
                {
                    _globalStorage.Items.RemoveItems(items.Item, items.Quantity * validPositions.Count);
                }

                if (Collection)
                    Collection.Add(validPositions);
                if (Tiles)
                    Tiles.Add(validPositions);
            }
        }
    }
}