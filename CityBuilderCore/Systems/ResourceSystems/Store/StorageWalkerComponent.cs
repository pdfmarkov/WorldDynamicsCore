using System;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// building component that stores items and has storage walkers to manage items as follows:<br/>
    /// I:      fill up on items that have been configured as <see cref="StorageOrderMode.Get"/><br/>
    /// II:     deliver to <see cref="IItemReceiver"/> that need items<br/>
    /// III:    get rid of items that have been configured as <see cref="StorageOrderMode.Empty"/><br/>
    /// </summary>
    public class StorageWalkerComponent : StorageComponent
    {
        public override string Key => "STG";

        [Tooltip("holds the storage walkers of this component that perform logistical jobs for it")]
        public ManualStorageWalkerSpawner StorageWalkers;

        private void Awake()
        {
            StorageWalkers.Initialize(Building, onFinished: storageWalkerReturned);
        }
        private void Start()
        {
            this.StartChecker(checkWorkers);
        }

        private void checkWorkers()
        {
            if (!StorageWalkers.HasWalker)
                return;

            if (!Building.IsWorking)
                return;

            if (!Building.HasAccessPoint(StorageWalkers.Prefab.PathType, StorageWalkers.Prefab.PathTag))
                return;

            //GET
            foreach (var order in Orders.Where(o => o.Mode == StorageOrderMode.Get).OrderBy(o => o.Item.Priority))
            {
                var capacity = GetReceiveCapacity(order.Item);
                if (capacity <= 0)
                    return;

                var giverPath = Dependencies.Get<IGiverPathfinder>().GetGiverPath(Building, null, new ItemQuantity(order.Item, order.Item.UnitSize), StorageWalkers.Prefab.MaxDistance, StorageWalkers.Prefab.PathType, StorageWalkers.Prefab.PathTag);
                if (giverPath == null)
                    continue;

                StorageWalkers.Spawn(walker => walker.StartGet(giverPath, new ItemQuantity(order.Item, capacity)));
            }
            //SUPPLY
            foreach (var items in Storage.GetItemQuantities().OrderBy(i => i.Item.Priority).ToList())
            {
                var receiverPath = Dependencies.Get<IReceiverPathfinder>().GetReceiverPath(Building, null, items, StorageWalkers.Prefab.MaxDistance, StorageWalkers.Prefab.PathType, StorageWalkers.Prefab.PathTag, Priority);
                if (receiverPath == null)
                    continue;

                StorageWalkers.Spawn(walker => walker.StartSupply(receiverPath, Storage, items.Item));
                return;
            }
            //EMPTY
            foreach (var order in Orders.Where(o => o.Mode == StorageOrderMode.Empty).OrderBy(o => o.Item.Priority))
            {
                int quantity = Storage.GetItemsOverRatio(order.Item, order.Ratio);
                if (quantity <= 0)
                    return;

                var receiverPath = Dependencies.Get<IReceiverPathfinder>().GetReceiverPath(Building, null, new ItemQuantity(order.Item, quantity), StorageWalkers.Prefab.MaxDistance, StorageWalkers.Prefab.PathType, StorageWalkers.Prefab.PathTag);
                if (receiverPath == null)
                    continue;

                StorageWalkers.Spawn(walker => walker.StartEmpty(receiverPath, Storage, order.Item, quantity));
                return;
            }
        }

        private void storageWalkerReturned(StorageWalker walker)
        {
            this.ReceiveAll(walker.Storage);
        }

        #region Saving
        [Serializable]
        public class StorageWalkerData
        {
            public ItemStorage.ItemStorageData Storage;
            public ManualWalkerSpawnerData SpawnerData;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new StorageWalkerData()
            {
                Storage = Storage.SaveData(),
                SpawnerData = StorageWalkers.SaveData()
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<StorageWalkerData>(json);

            Storage.LoadData(data.Storage);
            StorageWalkers.LoadData(data.SpawnerData);
        }
        #endregion
    }
}