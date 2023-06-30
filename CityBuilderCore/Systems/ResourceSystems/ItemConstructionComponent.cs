using CityBuilderCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderTown
{
    /// <summary>
    /// building component that waits for items to be delivered to it<br/>
    /// when all items have been delivered it terminates itself and places a different building in its stead<br/>
    /// an example can be found in CityBuilderCore.Tests/City/ResourceSystems/Construct/ConstructionDebugging
    /// </summary>
    public class ItemConstructionComponent : BuildingComponent, IItemReceiver, IItemOwner
    {
        [Tooltip("use ItemSpecific mode and set ItemCapacities to the items needed to build")]
        public ItemStorage ItemStorage;
        [Tooltip("the building that will be placed when storage is filled")]
        public Building FinishedBuilding;

        public override string Key => "CST";

        public int Priority => 2000;

        public BuildingComponentReference<IItemReceiver> ReceiverReference { get; set; }
        public IItemContainer ItemContainer => ItemStorage;

        BuildingComponentReference<IItemReceiver> IBuildingTrait<IItemReceiver>.Reference { get => ReceiverReference; set => ReceiverReference = value; }

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            ReceiverReference = registerTrait<IItemReceiver>(this);
        }
        public override void TerminateComponent()
        {
            base.TerminateComponent();

            deregisterTrait<IItemReceiver>(this);
        }

        public IEnumerable<Item> GetReceiveItems() => ItemStorage.ItemCapacities.Select(i => i.Item);
        public int GetReceiveCapacity(Item item) => ItemStorage.GetItemCapacityRemaining(item);
        public void ReserveCapacity(Item item, int quantity) => ItemStorage.ReserveCapacity(item, quantity);
        public void UnreserveCapacity(Item item, int quantity) => ItemStorage.UnreserveCapacity(item, quantity);
        public int Receive(ItemStorage storage, Item item, int quantity)
        {
            var remaining = quantity - storage.MoveItemsTo(ItemStorage, item, quantity);

            if (ItemStorage.GetItemQuantity() == ItemStorage.GetItemCapacity())
            {
                Building.Terminate();

                if (Building is ExpandableBuilding expandableBuilding)
                    Dependencies.Get<IBuildingManager>().Add(transform.position, transform.rotation, FinishedBuilding, b => ((ExpandableBuilding)b).Expansion = expandableBuilding.Expansion);
                else
                    Dependencies.Get<IBuildingManager>().Add(transform.position, transform.rotation, FinishedBuilding);
            }

            return remaining;
        }

        #region Saving
        [Serializable]
        public class ItemConstructionData
        {
            public ItemStorage.ItemStorageData StorageData;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new ItemConstructionData()
            {
                StorageData = ItemStorage.SaveData()
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<ItemConstructionData>(json);

            ItemStorage.LoadData(data.StorageData);
        }
        #endregion
    }
}
