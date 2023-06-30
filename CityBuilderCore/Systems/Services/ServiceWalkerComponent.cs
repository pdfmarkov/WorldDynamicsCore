using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// building component that periodically spawns a <see cref="ServiceWalker"/>
    /// </summary>
    public class ServiceWalkerComponent : BuildingComponent
    {
        public override string Key => "SVW";

        [Tooltip("holds the service walkers that periodically spawn, roam around and refill the service recipients thay pass")]
        public CyclicServiceWalkerSpawner ServiceWalkers;

        private void Awake()
        {
            ServiceWalkers.Initialize(Building);
        }
        private void Update()
        {
            if (Building.IsWorking)
                ServiceWalkers.Update();
        }

        #region Saving
        [Serializable]
        public class RiskWalkerData
        {
            public CyclicWalkerSpawnerData SpawnerData;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new RiskWalkerData() { SpawnerData = ServiceWalkers.SaveData() });
        }
        public override void LoadData(string json)
        {
            ServiceWalkers.LoadData(JsonUtility.FromJson<RiskWalkerData>(json).SpawnerData);
        }
        #endregion
    }
}