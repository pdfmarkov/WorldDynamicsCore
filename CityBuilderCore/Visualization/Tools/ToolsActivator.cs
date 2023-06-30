using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// helper for activating a tool from a <see cref="UnityEngine.UI.Toggle"/>
    /// </summary>
    [RequireComponent(typeof(BaseTool))]
    public class ToolsActivator : MonoBehaviour
    {
        private BaseTool _tool;

        private void Awake()
        {
            _tool = GetComponent<BaseTool>();
        }

        public void SetToolActive(bool active)
        {
            if (active)
                Dependencies.Get<IToolsManager>().ActivateTool(_tool);
            else
                Dependencies.Get<IToolsManager>().DeactivateTool(_tool);
        }
    }
}