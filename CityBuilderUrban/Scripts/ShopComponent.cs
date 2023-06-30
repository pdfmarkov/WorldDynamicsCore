﻿using CityBuilderCore;
using System;
using UnityEngine;

namespace CityBuilderUrban
{
    public class ShopComponent : BuildingComponent, IBuildingTrait<ShopComponent>, IItemOwner
    {
        public override string Key => "SHP";

        public BuildingComponentReference<ShopComponent> Reference { get; set; }

        public ItemStorage Storage;
        public ItemQuantity Price;

        public IItemContainer ItemContainer => Storage;

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            Reference = registerTrait(this);
        }
        public override void OnReplacing(IBuilding replacement)
        {
            base.OnReplacing(replacement);

            replaceTrait(this, replacement.GetBuildingComponent<ShopComponent>());
        }
        public override void TerminateComponent()
        {
            base.TerminateComponent();

            deregisterTrait(this);
        }

        public void Supply(ItemQuantity items)
        {
            int remainaing = Storage.AddItems(items.Item, items.Quantity);
            int quantity = Price.Quantity * (items.Quantity - remainaing);

            Dependencies.Get<IGlobalStorage>().Items.AddItems(Price.Item, quantity);
            Dependencies.Get<UrbanManager>().VisualizeMoney(Building.Root.position, quantity);
        }

        public void Purchase(ItemQuantity items)
        {
            Storage.RemoveItems(items.Item, items.Quantity);
        }

        #region Saving
        [Serializable]
        public class ShopData
        {
            public ItemStorage.ItemStorageData Storage;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new ShopData() { Storage = Storage.SaveData() });
        }
        public override void LoadData(string json)
        {
            Storage.LoadData(JsonUtility.FromJson<ShopData>(json).Storage);
        }
        #endregion
    }
}
