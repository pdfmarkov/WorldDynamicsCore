using UnityEngine;
using UnityEngine.EventSystems;

namespace CityBuilderCore
{
    /// <summary>
    /// base class for ui behaviours that display tooltips
    /// </summary>
    public abstract class TooltipOwnerBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ITooltipOwner
    {
        public virtual string TooltipName => null;
        public virtual string TooltipDescription => null;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipName == null)
                return;
            Dependencies.GetOptional<ITooltipManager>()?.Enter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipName == null)
                return;
            Dependencies.GetOptional<ITooltipManager>()?.Exit(this);
        }
    }
}
