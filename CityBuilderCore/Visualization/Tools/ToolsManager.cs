using UnityEngine;
using UnityEngine.UI;

namespace CityBuilderCore
{
    /// <summary>
    /// simple <see cref="IToolsManager"/> implementation that should suffice for most cases
    /// </summary>
    public class ToolsManager : MonoBehaviour, IToolsManager
    {
        [Tooltip("tool that is activated when the active one gets deactivated(most likely the selection tool)")]
        public BaseTool FallbackTool;
        [Tooltip("toggle group that contains all the tools, used to deactivate all tools on RMB")]
        public ToggleGroup ToggleGroup;

        public BaseTool ActiveTool => _activeTool;

        private BaseTool _activeTool;
        private IHighlightManager _highlighting;

        protected virtual void Awake()
        {
            Dependencies.Register<IToolsManager>(this);
        }

        private void Start()
        {
            _highlighting = Dependencies.Get<IHighlightManager>();

            if (!_activeTool && FallbackTool)
                ActivateTool(FallbackTool);
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(1) && _activeTool && !_activeTool.IsTouchActivated)
            {
                if (ToggleGroup)
                    ToggleGroup.SetAllTogglesOff(false);
                DeactivateTool(_activeTool);
            }
        }

        public void ActivateTool(BaseTool tool)
        {
            if (_activeTool)
                _activeTool.DeactivateTool();
            _highlighting?.Clear();
            _activeTool = tool ? tool : FallbackTool;
            if (_activeTool)
                _activeTool.ActivateTool();
        }

        public void DeactivateTool(BaseTool tool)
        {
            if (_activeTool != tool)
                return;

            ActivateTool(null);
        }

        public int GetCost(Item item) => _activeTool ? _activeTool.GetCost(item) : 0;
    }
}