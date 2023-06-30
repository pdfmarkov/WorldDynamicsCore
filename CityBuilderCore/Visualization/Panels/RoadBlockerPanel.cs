﻿using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// container in unity ui that generates toggles for visualizing and editing which tags a roadblocker blocks/>
    /// </summary>
    public class RoadBlockerPanel : MonoBehaviour
    {
        [Tooltip("instantiated for every tag in the road blocker as child of the panel")]
        public RoadBlockerToggle TogglePrefab;

        private RoadBlockerComponent _roadBlockerComponent;
        private List<RoadBlockerToggle> _tagToggles = new List<RoadBlockerToggle>();

        public void SetBlocker(RoadBlockerComponent roadBlockerComponent)
        {
            _tagToggles.ForEach(t => Destroy(t.gameObject));
            _tagToggles.Clear();

            _roadBlockerComponent = roadBlockerComponent;

            if (_roadBlockerComponent == null)
                return;

            foreach (var tag in _roadBlockerComponent.Tags)
            {
                var tagToggle = Instantiate(TogglePrefab, transform);

                tagToggle.Initialize(_roadBlockerComponent, tag);

                _tagToggles.Add(tagToggle);
            }
        }
    }
}