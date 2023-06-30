﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// shows all the different factors that go into the layer value of a point on the map<br/>
    /// gets automatically activated by <see cref="DefaultOverlayManager"/> when a layerview is shown
    /// </summary>
    public class LayerKeyVisualizer : MonoBehaviour
    {
        [Tooltip("root that will be moved around with the mouse")]
        public RectTransform Root;
        [Tooltip("object that gets activated and deactivated when the visualizer is shown/hidden")]
        public GameObject Visual;
        [Tooltip("prefab for one line in the visualizer(used for modifiers and affectors)")]
        public LayerAffectorVisualizer AffectorPrefab;

        [Tooltip("object that gets activated when the current point has a base value other than 0")]
        public GameObject BaseValueObject;
        [Tooltip("labal that the base value gets shown in")]
        public TMPro.TMP_Text BaseValueText;
        [Tooltip("label for the total value")]
        public TMPro.TMP_Text TotalValueText;

        private ILayerManager _layerManager;
        private IMouseInput _mouseInput;
        private IMap _map;

        private Vector2Int _activeMousePosition;
        private Layer _activeLayer;
        private List<LayerAffectorVisualizer> _affectorVisualizers = new List<LayerAffectorVisualizer>();
        private Canvas _currentCanvas;

        private void Start()
        {
            _layerManager = Dependencies.Get<ILayerManager>();
            _mouseInput = Dependencies.Get<IMouseInput>();
            _map = Dependencies.Get<IMap>();
            _currentCanvas = GetComponentInParent<Canvas>();

            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!_activeLayer)
                return;

            if (_currentCanvas)
                Root.anchoredPosition = _mouseInput.GetMouseScreenPosition() / _currentCanvas.scaleFactor;
            else
                Root.anchoredPosition = _mouseInput.GetMouseScreenPosition();

            var currentMousePositon = _mouseInput.GetMouseGridPosition();
            if (currentMousePositon == _activeMousePosition)
                return;
            _activeMousePosition = currentMousePositon;

            _affectorVisualizers.ForEach(a => Destroy(a.gameObject));
            _affectorVisualizers.Clear();

            if (!_map.IsInside(_activeMousePosition))
            {
                Visual.SetActive(false);
                return;
            }

            var key = _layerManager.GetKey(_activeLayer, _activeMousePosition);

            if (key == null)
            {
                Visual.SetActive(false);
                return;
            }

            Visual.SetActive(true);
            BaseValueObject.SetActive(key.BaseValue != 0);
            BaseValueText.text = key.BaseValue.ToString();
            TotalValueText.text = key.TotalValue.ToString();

            for (int i = 0; i < key.Affectors.Count; i++)
            {
                var affector = key.Affectors.ElementAt(i);

                addAffector(affector.Item2.Name, affector.Item1, i);
            }

            for (int i = 0; i < key.Modifiers.Count; i++)
            {
                var modifier = key.Modifiers.ElementAt(i);

                addAffector(modifier.Item2.Name, modifier.Item1, key.Affectors.Count + i);
            }
        }

        public void Activate(Layer layer)
        {
            _activeLayer = layer;
            _activeMousePosition = new Vector2Int(int.MaxValue, int.MaxValue);
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            _activeLayer = null;
            _activeMousePosition = new Vector2Int(int.MaxValue, int.MaxValue);
            gameObject.SetActive(false);
        }

        private void addAffector(string name, int value, int index)
        {
            var affectorVisualizer = Instantiate(AffectorPrefab, BaseValueObject.transform.parent);
            affectorVisualizer.NameText.text = name;
            affectorVisualizer.ValueText.text = value.ToString();
            affectorVisualizer.transform.SetSiblingIndex(index + 1);
            affectorVisualizer.gameObject.SetActive(true);
            _affectorVisualizers.Add(affectorVisualizer);
        }
    }
}
