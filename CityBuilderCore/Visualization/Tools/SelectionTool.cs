using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CityBuilderCore
{
    /// <summary>
    /// selects walkers and buildings under the mouse on click
    /// </summary>
    public class SelectionTool : BaseTool
    {
        [Tooltip("fired when a building is clicked, use to show building dialogs and such")]
        public BuildingEvent BuildingSelected;
        [Tooltip("fired when a walker is clicked, use to show walker dialogs and such")]
        public WalkerEvent WalkerSelected;
        [Tooltip("fired when a click occured but not building or walker was found")]
        public Vector2IntEvent PointSelected;

        public override bool ShowGrid => false;
        public override bool IsTouchPanAllowed => true;

        private IMouseInput _mouseInput;

        private void Start()
        {
            _mouseInput = Dependencies.Get<IMouseInput>();
        }

        protected override void updateTool()
        {
            base.updateTool();

            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                onApplied();

                var mousePosition = _mouseInput.GetMouseGridPosition();

                var walkerObject = Physics.RaycastAll(_mouseInput.GetRay()).Select(h => h.transform.gameObject).FirstOrDefault(g => g.CompareTag("Walker"));
                if (walkerObject)
                {
                    var walker = walkerObject.GetComponent<Walker>();
                    if (!walker)
                        walker = walkerObject.GetComponentInParent<Walker>();

                    if (walker)
                    {
                        WalkerSelected?.Invoke(walker);
                        return;
                    }
                }

                var building = Dependencies.Get<IBuildingManager>().GetBuilding(mousePosition).FirstOrDefault();
                if (building != null)
                {
                    BuildingSelected?.Invoke(building.BuildingReference);
                    return;
                }

                PointSelected?.Invoke(mousePosition);
            }
        }
    }
}