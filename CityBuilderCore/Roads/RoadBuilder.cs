using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// tool for placing roads
    /// </summary>
    public class RoadBuilder : PointerToolBase
    {
        [Tooltip("the road that will be placed by this builder")]
        public Road Road;
        public bool Pathfinding;
        public Object PathfindingTag;

        public override string TooltipName => Road.Cost != null && Road.Cost.Length > 0 ? $"{Road.Name}({Road.Cost.ToDisplayString()})" : Road.Name;
        
        private List<ItemQuantity> _costs = new List<ItemQuantity>();
        private IGlobalStorage _globalStorage;
        private IHighlightManager _highlighting;

        protected override void Start()
        {
            base.Start();

            _globalStorage = Dependencies.GetOptional<IGlobalStorage>();
            _highlighting = Dependencies.Get<IHighlightManager>();
        }

        public override void ActivateTool()
        {
            base.ActivateTool();

            checkCost(1);
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

            IEnumerable<Vector2Int> points;

            if (isDown)
            {
                if(Pathfinding)
                {
                    var path = PathHelper.FindPath(dragStart, mousePoint, PathType.MapGrid, PathfindingTag);
                    if (path == null)
                        points = PositionHelper.GetRoadPositions(dragStart, mousePoint);
                    else
                        points = path.GetPoints().ToList();
                }
                else
                {
                    points = PositionHelper.GetRoadPositions(dragStart, mousePoint);
                }
            }
            else if (IsTouchActivated)
            {
                points = new Vector2Int[] { };
            }
            else
            {
                points = new Vector2Int[] { mousePoint };
            }

            foreach (var position in points)
            {
                if (Dependencies.Get<IStructureManager>().CheckAvailability(position, Road.Level.Value))
                {
                    validPositions.Add(position);
                }
                else
                {
                    invalidPositions.Add(position);
                }
            }

            if (!checkCost(validPositions.Count))
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

                if (_globalStorage != null)
                {
                    foreach (var items in Road.Cost)
                    {
                        _globalStorage.Items.RemoveItems(items.Item, items.Quantity * validPositions.Count);
                    }
                }

                Dependencies.Get<IRoadManager>().Add(validPositions, Road);
            }
        }

        private bool checkCost(int count)
        {
            bool hasCost = true;
            _costs.Clear();
            foreach (var items in Road.Cost)
            {
                _costs.Add(new ItemQuantity(items.Item, items.Quantity * count));

                if (_globalStorage != null && !_globalStorage.Items.HasItemsRemaining(items.Item, items.Quantity * count))
                {
                    hasCost = false;
                }
            }
            return hasCost;
        }
    }
}