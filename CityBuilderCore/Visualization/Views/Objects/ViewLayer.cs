﻿using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// view that displays an overlay for a layer on a tilemap
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Views/" + nameof(ViewLayer))]
    public class ViewLayer : View
    {
        [Tooltip("the layer to visualize with this view")]
        public Layer Layer;
        [Tooltip("gradient that determines the color for any layer value that will be displayed as an overlay")]
        public Gradient Gradient;
        [Tooltip("layer value for which the lowest gradient value will be used")]
        public int Minimum;
        [Tooltip("layer value at which the highest gradient value will be used")]
        public int Maximum;

        public override void Activate() => Dependencies.Get<IOverlayManager>().ActivateOverlay(this);
        public override void Deactivate() => Dependencies.Get<IOverlayManager>().ClearOverlay();
    }
}