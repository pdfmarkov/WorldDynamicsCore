﻿using System.Globalization;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// building component that replaces the building after a defined time has passed
    /// </summary>
    public class TimedReplacementComponent : BuildingComponent
    {
        public override string Key => "TRP";

        [Tooltip("time that is counted down until the building is replaced")]
        public float Duration;
        [Tooltip("the building that will take the current buildings place after the duration has passed")]
        public Building Prefab;

        private float _passed;

        private void Update()
        {
            _passed += Time.deltaTime * Building.Efficiency;
            if (_passed >= Duration)
                Building.Replace(Prefab.Info.GetPrefab(Building.Index));
        }

        #region Saving
        public override string SaveData()
        {
            return _passed.ToString(CultureInfo.InvariantCulture);
        }
        public override void LoadData(string json)
        {
            _passed = float.Parse(json, CultureInfo.InvariantCulture);
        }
        #endregion
    }
}
