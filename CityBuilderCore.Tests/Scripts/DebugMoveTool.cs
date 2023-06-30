using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CityBuilderCore
{
    /// <summary>
    /// can be used to move walkers for debugging
    /// </summary>
    public class DebugMoveTool : BaseTool
    {
        public override bool ShowGrid => false;
        public override bool IsTouchPanAllowed => true;

        public Transform Marker;

        private IMouseInput _mouseInput;
        private IHighlightManager _highlights;
        private Walker _selectedWalker;

        private Action _nextMove;

        private void Start()
        {
            _mouseInput = Dependencies.Get<IMouseInput>();
            _highlights = Dependencies.GetOptional<IHighlightManager>();
        }

        protected override void updateTool()
        {
            base.updateTool();

            var mousePosition = _mouseInput.GetMouseGridPosition();

            if (_highlights != null)
            {
                _highlights.Clear();
                _highlights.Highlight(mousePosition, HighlightType.Info);
            }

            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var walkerObject = Physics.RaycastAll(_mouseInput.GetRay()).Select(h => h.transform.gameObject).FirstOrDefault(g => g.CompareTag("Walker"));
                if (walkerObject)
                {
                    var walker = walkerObject.GetComponent<Walker>();
                    if (walker)
                    {
                        _selectedWalker = walker;

                        Marker.SetParent(_selectedWalker.Pivot, false);
                    }
                }

                onApplied();
            }

            if (Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (_selectedWalker != null)
                {
                    var building = Dependencies.Get<IBuildingManager>().GetBuilding(mousePosition).FirstOrDefault();
                    if (building == null)
                    {
                        _nextMove = () => _selectedWalker.walk(mousePosition, checkNext);
                    }
                    else
                    {
                        _nextMove = () => _selectedWalker.walk(building, checkNext);
                    }

                    checkNext();
                }

                onApplied();
            }
        }

        private void checkNext()
        {
            if (_selectedWalker == null || _nextMove == null)
                return;

            if (_selectedWalker.IsWalking)
            {
                _selectedWalker.cancelWalk();
            }
            else
            {
                var move = _nextMove;
                _nextMove = null;//avoid stack overflow if move finishes immediately
                move();
            }
        }
    }
}