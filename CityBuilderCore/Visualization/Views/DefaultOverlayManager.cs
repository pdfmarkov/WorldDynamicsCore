using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CityBuilderCore
{
    /// <summary>
    /// default implementation of <see cref="IOverlayManager"/><br/>
    /// shows layer overlay using the color in <see cref="ViewLayer.Gradient"/>for tiles in a tilemap<br/>
    /// can also display a explanation for the values using <see cref="LayerKeyVisualizer"/>
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class DefaultOverlayManager : MonoBehaviour, IOverlayManager
    {
        [Tooltip("neutral tile that gets coloured")]
        public TileBase Tile;
        [Tooltip("optional visualizer that explains how the layer value on a point are calculated")]
        public LayerKeyVisualizer LayerKeyVisualizer;

        private Tilemap _tilemap;
        private ViewEfficiency _currentEfficiencyView;
        private ViewLayer _currentLayerView;

        protected virtual void Awake()
        {
            Dependencies.Register<IOverlayManager>(this);
        }

        protected virtual void Start()
        {
            _tilemap = GetComponent<Tilemap>();

            Dependencies.Get<ILayerManager>().Changed += layerChanged;
        }

        public void ActivateOverlay(ViewLayer view)
        {
            var range = view.Maximum - view.Minimum;
            var bottom = -view.Minimum;

            foreach (var value in Dependencies.Get<ILayerManager>().GetValues(view.Layer))
            {
                setTile((Vector3Int)value.Item1, view.Gradient.Evaluate((float)(value.Item2 + bottom) / range));
            }

            _currentLayerView = view;

            if (LayerKeyVisualizer)
                LayerKeyVisualizer.Activate(view.Layer);
        }

        public void ActivateOverlay(ViewEfficiency view)
        {
            _currentEfficiencyView = view;

            refreshEfficiencyOverlay();
            this.StartChecker(refreshEfficiencyOverlay);
        }

        public void ClearOverlay()
        {
            StopAllCoroutines();

            _tilemap.ClearAllTiles();

            _currentLayerView = null;
            _currentEfficiencyView = null;

            if (LayerKeyVisualizer)
                LayerKeyVisualizer.Deactivate();
        }

        private void layerChanged(Layer layer)
        {
            if (_currentLayerView && _currentLayerView.Layer == layer)
                refreshLayerOverlay();
        }

        private void refreshLayerOverlay()
        {
            var current = _currentLayerView;
            ClearOverlay();
            _currentLayerView = current;
            ActivateOverlay(_currentLayerView);
        }

        private void refreshEfficiencyOverlay()
        {
            _tilemap.ClearAllTiles();

            var buildingManager = Dependencies.Get<IBuildingManager>();
            foreach (var building in buildingManager.GetBuildings().Where(b => b.HasBuildingPart<IEfficiencyFactor>()))
            {
                var efficiency = building.Efficiency;
                foreach (var point in PositionHelper.GetStructurePositions(building.Point, building.Size))
                {
                    setTile((Vector3Int)point, _currentEfficiencyView.Gradient.Evaluate(efficiency));
                }
            }
        }

        private void setTile(Vector3Int point, Color color)
        {
            _tilemap.SetTile(point, Tile);
            _tilemap.SetTileFlags(point, TileFlags.None);
            _tilemap.SetColor(point, color);
        }
    }
}