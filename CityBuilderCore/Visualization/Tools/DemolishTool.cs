using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// tool that removes structures
    /// </summary>
    public class DemolishTool : PointerToolBase
    {
        [Tooltip("is displayed as its tooltip")]
        public string Name;
        [Tooltip("optional effect that gets added for removed buildings")]
        public DemolishVisual Visual;
        [Tooltip("determines which structures are affected by this tool")]
        public StructureLevelMask Level;

        public override string TooltipName => Name;

        private IHighlightManager _highlighting;

        protected override void Start()
        {
            base.Start();

            _highlighting = Dependencies.Get<IHighlightManager>();
        }

        protected override void updatePointer(Vector2Int mousePoint, Vector2Int dragStart, bool isDown, bool isApply)
        {
            _highlighting.Clear();

            IEnumerable<Vector2Int> points;

            if (isDown)
            {
                points = PositionHelper.GetBoxPositions(dragStart, mousePoint);
            }
            else
            {
                if (IsTouchActivated)
                    points = new Vector2Int[] { };
                else
                    points = new Vector2Int[] { mousePoint };
            }

            _highlighting.Highlight(points, false);

            if (isApply)
            {
                int count = Dependencies.Get<IStructureManager>().Remove(points, Level.Value, false, structure =>
                   {
                       DemolishVisual.Create(Visual, structure as IBuilding);
                   });

                if (count > 0)
                    onApplied();
            }
        }
    }
}