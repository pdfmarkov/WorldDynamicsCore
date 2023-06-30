using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// sub container for item 'slots' for storages in <see cref="ItemStorageMode.Stacked"/><br/>
    /// a stacked storage can define a number of stacks<br/>
    /// each stack can only contain one type of item<br/>
    /// makes for some nicely visualized storages
    /// </summary>
    [Serializable]
    public class ItemStack
    {
        [Tooltip("how many (storage)units the stack can hold, how many items that is depends on the items UnitSize")]
        public int UnitCapacity;

        public ItemQuantity Items { get; private set; }
        public bool HasItems => Items != null && Items.Item != null;
        public float FillDegree => HasItems ? (float)Items.Quantity / (UnitCapacity * Items.Item.UnitSize) : 0f;

        public event Action<ItemStack> Changed;

        public int GetItemCapacity(Item item)
        {
            return UnitCapacity * item.UnitSize;
        }
        public int GetItemCapacityRemaining(Item item)
        {
            if (HasItems)
            {
                if (Items.Item != item)
                    return 0;

                return UnitCapacity * item.UnitSize - Items.Quantity;
            }
            else
            {
                return UnitCapacity * item.UnitSize;
            }
        }

        public int SubtractQuantity(Item item, int amount)
        {
            if (!HasItems || Items.Item != item)
                return amount;

            if (amount >= Items.Quantity)
            {
                amount -= Items.Quantity;
                Items = null;
            }
            else
            {
                Items.Quantity -= amount;
                amount = 0;
            }

            onChanged();

            return amount;
        }

        public int AddQuantity(Item item, int amount)
        {
            int capacity = UnitCapacity * item.UnitSize;

            if (HasItems)
            {
                if (Items.Item != item)
                    return amount;

                int added = Mathf.Min(capacity - Items.Quantity, amount);

                Items.Quantity += added;

                onChanged();

                return amount - added;
            }
            else
            {
                Items = new ItemQuantity()
                {
                    Item = item,
                    Quantity = Mathf.Min(capacity, amount)
                };

                onChanged();

                return amount - Items.Quantity;
            }
        }

        public void SetQuantity(int amount)
        {
            if (amount == 0)
                Items = null;
            else
                Items.Quantity = amount;

            onChanged();
        }

        private void onChanged() => Changed?.Invoke(this);

        #region Saving
        public ItemQuantity.ItemQuantityData GetData()
        {
            if (HasItems)
            {
                return Items.GetData();
            }
            else
            {
                return new ItemQuantity.ItemQuantityData();
            }
        }
        public void SetData(ItemQuantity.ItemQuantityData data)
        {
            Items = ItemQuantity.FromData(data);
            onChanged();
        }
        #endregion
    }
}