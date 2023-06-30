using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// building component that periodically consumes and produces items<br/>
    /// production time is only started once the consumption items are all there<br/>
    /// consumption items have to be provided by others, produced items get shipped with <see cref="DeliveryWalker"/>
    /// </summary>
    public class VariantProductionWalkerComponent : VariantProductionComponent
    {
        [Tooltip("walkers that spawn with produced items and try to deliver items to receivers")]
        public ManualDeliveryWalkerSpawner DeliveryWalkers;

        protected override void Awake()
        {
            base.Awake();

            DeliveryWalkers.Initialize(Building);
        }
        protected override void Start()
        {
            base.Awake();

            this.StartChecker(checkDelivery);
        }

        private void checkDelivery()
        {
            if (!DeliveryWalkers.HasWalker)
                return;

            if (!Building.IsWorking)
                return;

            if (!Building.HasAccessPoint(DeliveryWalkers.Prefab.PathType, DeliveryWalkers.Prefab.PathTag))
                return;

            foreach (var item in GetItemsOut())
            {
                if (!Storage.HasItem(item))
                    continue;

                DeliveryWalkers.Spawn(walker =>
                {
                    walker.StartDelivery(Storage, item);
                });
            }
        }

        #region Saving
        [Serializable]
        public class VariantProductionWalkerData : VariantProductionData
        {
            public ManualWalkerSpawnerData SpawnerData;
        }

        public override string SaveData()
        {
            var data = new VariantProductionWalkerData();

            saveData(data);

            data.SpawnerData = DeliveryWalkers.SaveData();

            return JsonUtility.ToJson(data);
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<VariantProductionWalkerData>(json);

            loadData(data);

            DeliveryWalkers.LoadData(data.SpawnerData);
        }
        #endregion
    }
}