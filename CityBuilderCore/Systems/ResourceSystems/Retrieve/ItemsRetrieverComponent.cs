using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// periodically spawns <see cref="RetrieverWalkers"/> to get items from dispensers<br/>
    /// the retrieved items are either stored in global storage(set <see cref="Storage"/> to Global) or distributed using a <see cref="DeliveryWalker"/><br/>
    /// (eg hunter or lumberjack hut)
    /// </summary>
    public class ItemsRetrieverComponent : BuildingComponent, IItemOwner
    {
        public override string Key => "ITR";

        [Tooltip("stores items until they are delivered(unless it just stores them globally)")]
        public ItemStorage Storage;

        [Tooltip("spawns walkers on an interval that get items from the closest dispenser")]
        public CyclicItemsRetrieverWalkerSpawner RetrieverWalkers;
        [Tooltip("optional walkers that deliver the dispensed items to a receiver")]
        public ManualDeliveryWalkerSpawner DeliveryWalkers;

        public IItemContainer ItemContainer => Storage;

        private void Awake()
        {
            RetrieverWalkers.Initialize(Building, walkerLeaving, walkerReturned);
            DeliveryWalkers.Initialize(Building);
        }
        private void Start()
        {
            if (Storage.Mode != ItemStorageMode.Global && DeliveryWalkers.Prefab)
                this.StartChecker(checkDelivery);
        }
        private void Update()
        {
            if (Building.IsWorking)
                RetrieverWalkers.Update();
        }

        public override string GetDebugText() => Storage.GetDebugText();

        private bool walkerLeaving(ItemsRetrieverWalker walker)
        {
            var dispenser = Dependencies.Get<IItemsDispenserManager>().GetDispenser(RetrieverWalkers.Prefab.DispenserKey, Building.WorldCenter, RetrieverWalkers.Prefab.MaxDistance);
            if (dispenser == null)
                return false;
            walker.StartRetrieving(dispenser);
            return true;
        }
        private void walkerReturned(ItemsRetrieverWalker walker)
        {
            walker.Storage.MoveItemsTo(Storage);
        }

        private void checkDelivery()
        {
            if (!DeliveryWalkers.HasWalker)
                return;

            if (!Building.IsWorking)
                return;

            if (!Storage.HasItems())
                return;

            if (!Building.HasAccessPoint(DeliveryWalkers.Prefab.PathType, DeliveryWalkers.Prefab.PathTag))
                return;

            DeliveryWalkers.Spawn(walker =>
            {
                walker.StartDelivery(Storage);
            });
        }

        #region Saving
        [Serializable]
        public class ItemsRetrieverData
        {
            public ItemStorage.ItemStorageData Storage;
            public CyclicWalkerSpawnerData RetrieverSpawnerData;
            public ManualWalkerSpawnerData DeliverySpawnerData;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new ItemsRetrieverData()
            {
                Storage = Storage.SaveData(),
                RetrieverSpawnerData = RetrieverWalkers.SaveData(),
                DeliverySpawnerData = DeliveryWalkers.SaveData(),
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<ItemsRetrieverData>(json);

            Storage.LoadData(data.Storage);
            RetrieverWalkers.LoadData(data.RetrieverSpawnerData);
            DeliveryWalkers.LoadData(data.DeliverySpawnerData);
        }
        #endregion
    }
}
