﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// tool for placing buildings
    /// </summary>
    public class BuildingBuilder : PointerToolBase
    {
        [Tooltip("info of the building that will be placed, used to get the ghost and check building requirements")]
        public BuildingInfo BuildingInfo;
        [Tooltip("whether buildings can be rotated using Q, for example in Isometric games where this does not make sense")]
        public bool AllowRotate = true;

        public override string TooltipName => BuildingInfo.Cost != null && BuildingInfo.Cost.Length > 0 ? $"{BuildingInfo.Name}({BuildingInfo.Cost.ToDisplayString()})" : BuildingInfo.Name;
        public override string TooltipDescription => BuildingInfo.Description;

        private BuildingRotation _rotation;
        private int _index;
        private GameObject _ghost;

        private List<ItemQuantity> _costs = new List<ItemQuantity>();
        private IGlobalStorage _globalStorage;
        private IHighlightManager _highlighting;
        private IMap _map;
        private IMainCamera _mainCamera;
        private IGridPositions _gridPositions;
        private IGridHeights _gridHeights;

        protected override void Start()
        {
            base.Start();

            _globalStorage = Dependencies.GetOptional<IGlobalStorage>();
            _highlighting = Dependencies.Get<IHighlightManager>();
            _map = Dependencies.Get<IMap>();
            _mainCamera = Dependencies.GetOptional<IMainCamera>();

            _gridPositions = Dependencies.Get<IGridPositions>();
            _gridHeights = Dependencies.GetOptional<IGridHeights>();
        }

        public override void ActivateTool()
        {
            base.ActivateTool();

            _index = 0;
            _rotation = Dependencies.GetOptional<BuildingRotationKeeper>()?.Rotation ?? BuildingRotation.Create();

            recreateGhost();

            checkCost(1);
        }

        public override void DeactivateTool()
        {
            if (_ghost)
            {
                Destroy(_ghost);
            }

            _costs.Clear();

            base.DeactivateTool();
        }

        public override int GetCost(Item item)
        {
            return _costs.FirstOrDefault(c => c.Item == item)?.Quantity ?? 0;
        }

        protected override void updateTool()
        {
            base.updateTool();

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _index++;
                recreateGhost();
            }
        }

        protected override void updatePointer(Vector2Int mousePoint, Vector2Int dragStart, bool isDown, bool isApply)
        {
            if (!isDown)
            {
                if (AllowRotate && Input.GetKeyDown(KeyCode.R))
                {
                    _rotation.TurnClockwise();
                }
            }

            List<Vector2Int> validPoints = new List<Vector2Int>();
            List<Vector2Int> invalidPoints = new List<Vector2Int>();

            Vector2Int size = _rotation.RotateSize(BuildingInfo.Size);
            List<Vector2Int> buildPoints;
            List<Vector2Int> validBuildPoints = new List<Vector2Int>();

            if (_mainCamera != null && !_map.IsHex)
            {
                //hold the building by a different edge when the camera is rotated

                var angle = _mainCamera.Rotation.eulerAngles.y;
                var offset = Vector2Int.zero;

                if (_map.IsXY)
                {
                    if (angle > 270)
                        offset = new Vector2Int(size.x - 1, 0);
                    else if (angle > 180)
                        offset = new Vector2Int(size.x - 1, size.y - 1);
                    else if (angle > 90)
                        offset = new Vector2Int(0, size.y - 1);
                }
                else
                {
                    if (angle > 270)
                        offset = new Vector2Int(size.x - 1, 0);
                    else if (angle > 180)
                        offset = new Vector2Int(size.x - 1, size.y - 1);
                    else if (angle > 90)
                        offset = new Vector2Int(0, size.y - 1);
                }

                mousePoint -= offset;
                dragStart -= offset;
            }

            if (isDown)
            {
                if (IsTouchActivated)
                    buildPoints = new List<Vector2Int>() { mousePoint };
                else
                    buildPoints = PositionHelper.GetBoxPositions(dragStart, mousePoint, size).ToList();
            }
            else
            {
                if (IsTouchActivated)
                    buildPoints = new List<Vector2Int>() { };
                else
                    buildPoints = new List<Vector2Int>() { mousePoint };
            }

            foreach (var buildPoint in buildPoints)
            {
                var structurePoints = PositionHelper.GetStructurePositions(buildPoint, size);
                var isInside = structurePoints.All(p => _map.IsInside(p));
                var isFulfillingRequirements = isInside && BuildingInfo.CheckBuildingRequirements(buildPoint, _rotation);
                var isCompletelyValid = true;

                foreach (var point in structurePoints)
                {
                    if (isFulfillingRequirements && BuildingInfo.CheckBuildingAvailability(point))
                    {
                        validPoints.Add(point);
                    }
                    else
                    {
                        invalidPoints.Add(point);
                        isCompletelyValid = false;
                    }
                }

                if (isCompletelyValid)
                {
                    validBuildPoints.Add(buildPoint);
                }

                if (buildPoints.IndexOf(buildPoint) == 0 && _ghost)
                {
                    _ghost.SetActive(isCompletelyValid);
                    _ghost.transform.position = _gridPositions.GetWorldPosition(_rotation.RotateOrigin(buildPoint, BuildingInfo.Size));
                    _ghost.transform.rotation = _rotation.GetRotation();

                    if (_gridHeights != null)
                        _gridHeights.ApplyHeight(_ghost.transform, _gridPositions.GetWorldCenterPosition(mousePoint, size));
                }
            }

            if (!checkCost(Mathf.Max(1, validBuildPoints.Count)))
            {
                invalidPoints.AddRange(validPoints);
                validPoints.Clear();
                validBuildPoints.Clear();
            }

            _highlighting.Clear();
            if (BuildingInfo.AccessType != BuildingAccessType.Any && buildPoints.Any())
                _highlighting.Highlight(_rotation.RotateBuildingPoint(buildPoints.Last(), BuildingInfo.AccessPoint, BuildingInfo.Size), HighlightType.Info);
            _highlighting.Highlight(validPoints, true);
            _highlighting.Highlight(invalidPoints, false);

            if (isApply)
            {
                build(validBuildPoints);
                recreateGhost();
            }
        }

        private void recreateGhost()
        {
            if (_ghost)
                Destroy(_ghost);

            var prefab = BuildingInfo.GetGhost(_index);

            if (prefab)
            {
                _ghost = Instantiate(prefab);
                _ghost.SetActive(false);
            }
        }

        private bool checkCost(int count)
        {
            bool hasCost = true;
            _costs.Clear();
            foreach (var items in BuildingInfo.Cost)
            {
                _costs.Add(new ItemQuantity(items.Item, items.Quantity * count));
                if (_globalStorage != null && !_globalStorage.Items.HasItemsRemaining(items.Item, items.Quantity * count))
                {
                    hasCost = false;
                }
            }
            return hasCost;
        }

        private void build(IEnumerable<Vector2Int> points)
        {
            var buildingManager = Dependencies.Get<IBuildingManager>();

            if (points.Any())
                onApplied();

            foreach (var point in points)
            {
                if (_globalStorage != null)
                {
                    foreach (var items in BuildingInfo.Cost)
                    {
                        _globalStorage.Items.RemoveItems(items.Item, items.Quantity);
                    }
                }

                BuildingInfo.Prepare(point, _rotation);

                buildingManager.Add(_gridPositions.GetWorldPosition(_rotation.RotateOrigin(point, BuildingInfo.Size)), _rotation.GetRotation(), BuildingInfo.GetPrefab(_index));

                _index++;
            }
        }
    }
}