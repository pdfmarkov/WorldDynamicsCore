using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// can be put on any raycast UI objects to display a manually set tooltip
    /// </summary>
    public class TooltipArea : TooltipOwnerBase
    {
        [Tooltip("main text displayed when the pointer remains over ui objects")]
        public string Name;
        [Tooltip("secondary text displayed when the pointer remains over the ui object")]
        public string Description;

        public override string TooltipName => Name;
        public override string TooltipDescription => Description;
    }
}
