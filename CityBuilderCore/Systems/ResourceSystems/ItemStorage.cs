using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// general container for storage of items, use whenever anything owns items<br/>
    /// defines various convenience methods for moving and checking items that abstract its different storage modes
    /// </summary>
    [Serializable]
    public class ItemStorage : IItemContainer
    {
        private ItemStorage _globalStorage => Dependencies.Get<IGlobalStorage>().Items;

        public bool IsStackedStorage => Mode == ItemStorageMode.Stacked;
        public bool IsGlobalStorage => Mode == ItemStorageMode.Global;
        public bool IsListStorage => !IsStackedStorage && !IsGlobalStorage;

        [Tooltip(@"Stacked		consists of several sub stacks
Free		no limit
ItemCapped	capacity is quantity per item
UnitCapped	capacity is units per item
Global		redirects to IGlobalStorage
ItemSpecific	capacity as specified in Items field
TotalItemCapped cap is total quantity across items
TotalUnitCapped cap is total units across items")]
        public ItemStorageMode Mode = ItemStorageMode.UnitCapped;
        [Tooltip("the number of stacks in stacked mode, otherwise irrelevant")]
        public int StackCount;
        [Tooltip("number of units in unitCapped, of quantity in itemCapped and units/stack in stacked mode")]
        public int Capacity = 1;
        [Tooltip("the items that can be stored in ItemSpecific storage mode")]
        public ItemQuantity[] ItemCapacities;

        public event Action<ItemStorage> Changed;

        public ItemStack[] Stacks
        {
            get
            {
                if (_stacks == null)
                    setupStacks();
                return _stacks;
            }
        }

        private ItemStack[] _stacks = null;
        private List<ItemQuantity> _listItems = new List<ItemQuantity>();
        private List<ItemQuantity> _reservedCapacity = new List<ItemQuantity>();
        private List<ItemQuantity> _reservedQuantity = new List<ItemQuantity>();
        private List<ItemQuantity> _overflowItems = new List<ItemQuantity>();

        /// <summary>
        /// removes all items and reservations
        /// </summary>
        public void Clear()
        {
            _listItems = new List<ItemQuantity>();
            _reservedCapacity = new List<ItemQuantity>();
            _reservedQuantity = new List<ItemQuantity>();
            setupStacks();
        }

        /// <summary>
        /// returns whether the strorage contains any items
        /// </summary>
        /// <returns>true if any items are stored</returns>
        public bool HasItems()
        {
            return GetItems().Any();
        }
        /// <summary>
        /// returns whether the strorage contains a particular items
        /// </summary>
        /// <param name="item">the item to check for</param>
        /// <returns>whether the storage has any of the specified item</returns>
        public bool HasItem(Item item)
        {
            return GetItems().Any(i => i == item);
        }
        /// <summary>
        /// returns whether the strorage contains any item in the category
        /// </summary>
        /// <param name="itemCategory">the item category to check for</param>
        /// <returns>whether the storage has any of the specified item</returns>
        public bool HasItem(ItemCategory itemCategory)
        {
            return GetItems().Any(i => itemCategory.Items.Contains(i));
        }
        /// <summary>
        /// checks if at least a certain quantity of an item is stored<br/>
        /// does not take reservations into account, for that use <see cref="HasItemsRemaining(Item, int)"/>
        /// </summary>
        /// <param name="itemQuantity">the quantity of item to check</param>
        /// <returns>if at least the quantity of the item is stored</returns>
        public bool HasItems(ItemQuantity itemQuantity) => HasItems(itemQuantity.Item, itemQuantity.Quantity);
        /// <summary>
        /// checks if at least a certain quantity of an item is stored<br/>
        /// does not take reservations into account, for that use <see cref="HasItemsRemaining(Item, int)"/>
        /// </summary>
        /// <param name="item">the item to look for</param>
        /// <param name="quantity">the minimum qantity</param>
        /// <returns>if at least the quantity of the item is stored</returns>
        public bool HasItems(Item item, int quantity)
        {
            return GetItemQuantity(item) >= quantity;
        }
        /// <summary>
        /// checks if at least a certain quantity of an item category is stored<br/>
        /// does not take reservations into account, for that use <see cref="HasItemsRemaining(Item, int)"/>
        /// </summary>
        /// <param name="itemCategoryQuantity">the item category and quantity to look for</param>
        /// <returns>if at least the quantity of the item is stored</returns>
        public bool HasItems(ItemCategoryQuantity itemCategoryQuantity) => HasItems(itemCategoryQuantity.ItemCategory, itemCategoryQuantity.Quantity);
        /// <summary>
        /// checks if at least a certain quantity of an item category is stored<br/>
        /// does not take reservations into account, for that use <see cref="HasItemsRemaining(Item, int)"/>
        /// </summary>
        /// <param name="itemCategory">the item category to look for</param>
        /// <param name="quantity">the minimum qantity</param>
        /// <returns>if at least the quantity of the item is stored</returns>
        public bool HasItems(ItemCategory itemCategory, int quantity)
        {
            return GetItemQuantity(itemCategory) >= quantity;
        }
        /// <summary>
        /// checks if at least all the quantities passed are stored
        /// </summary>
        /// <param name="itemQuantities">the quantities to check</param>
        /// <returns>if at least the quantity of the item is stored</returns>
        public bool HasItems(IEnumerable<ItemQuantity> itemQuantities) => itemQuantities.All(HasItems);
        /// <summary>
        /// checks if at least all the quantities passed are stored
        /// </summary>
        /// <param name="itemCategoryQuantities">the quantities to check</param>
        /// <returns>if at least the quantity of the item is stored</returns>
        public bool HasItems(IEnumerable<ItemCategoryQuantity> itemCategoryQuantities) => itemCategoryQuantities.All(HasItems);
        /// <summary>
        /// checks if at least a certain quantity of an item is stored and unreserved
        /// </summary>
        /// <param name="item">the item to look for</param>
        /// <param name="quantity">the minimum qantity</param>
        /// <returns>if at least the quantity of the item is stored and not reserved</returns>
        public bool HasItemsRemaining(Item item, int quantity)
        {
            return GetItemQuantity(item) - _reservedQuantity.GetItemQuantity(item) >= quantity;
        }
        /// <summary>
        /// checks if at least a certain quantity of an item category is stored and unreserved
        /// </summary>
        /// <param name="itemCategory">the item category to look for</param>
        /// <param name="quantity">the minimum qantity</param>
        /// <returns>if at least the quantity of the item is stored and not reserved</returns>
        public bool HasItemsRemaining(ItemCategory itemCategory, int quantity)
        {
            return GetItemQuantity(itemCategory) - _reservedQuantity.GetItemQuantity(itemCategory) >= quantity;
        }

        /// <summary>
        /// calculates the total number of items in the store
        /// </summary>
        /// <returns>total item count</returns>
        public int GetItemQuantity() => GetItemQuantities().Sum(i => i.Quantity);
        /// <summary>
        /// calculates the total number of a particular item stored
        /// </summary>
        /// <param name="item">the item to count</param>
        /// <returns>total quantity of item</returns>
        public int GetItemQuantity(Item item)
        {
            var items = GetItemQuantities().FirstOrDefault(i => i.Item == item);
            if (items == null)
                return 0;
            else
                return items.Quantity;
        }
        /// <summary>
        /// calculates the total number of items in a particular itemCategory(food, ...) stored
        /// </summary>
        /// <param name="itemCategory">the item category to count</param>
        /// <returns>total quantity of items in the ItemCategory</returns>
        public int GetItemQuantity(ItemCategory itemCategory) => itemCategory.Items.Sum(i => GetItemQuantity(i));
        /// <summary>
        /// calculates the number of units(<see cref="Item.UnitSize"/>) stored for an item<br/>
        /// ie apples are stored in units of 100, 150 apples are in store > 1.5 units<br/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public float GetUnitQuantity(Item item) => (float)GetItemQuantity(item) / item.UnitSize;

        /// <summary>
        /// calculates the total number of a particular item stored wihtout reservations
        /// </summary>
        /// <param name="item">the item to count</param>
        /// <returns>total quantity of unreserved item</returns>
        public int GetItemQuantityRemaining(Item item) => GetItemQuantity(item) - _reservedQuantity.GetItemQuantity(item);

        /// <summary>
        /// calculates the total number of items that may be stored
        /// </summary>
        /// <returns>total capacity</returns>
        public int GetItemCapacity()
        {
            switch (Mode)
            {
                case ItemStorageMode.Stacked:
                    return StackCount * Capacity;
                case ItemStorageMode.Free:
                    return int.MaxValue;
                case ItemStorageMode.ItemCapped:
                case ItemStorageMode.TotalItemCapped:
                    return Capacity;
                case ItemStorageMode.Global:
                    return _globalStorage.GetItemCapacity();
                case ItemStorageMode.ItemSpecific:
                    return ItemCapacities.GetTotalQuantity();
                case ItemStorageMode.UnitCapped:
                    //unit capped is tricky here, we dont know the capacity without having the item
                    //but we also cant return 0 because that indicates it cant have any items,
                    //returning capacity will at least be correct for items with unit size 1
                    return Capacity;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// calculates the total number that may be stored of a particular item
        /// </summary>
        /// <param name="item">item in question</param>
        /// <returns>total capacity for the item</returns>
        public int GetItemCapacity(Item item) => GetItemCapacity(item, 1f);
        /// <summary>
        /// calculates the proportional number that may be stored of a particular item
        /// </summary>
        /// <param name="item">item in question</param>
        /// <param name="ratio">how much of the capacity may be used (0-1)</param>
        /// <returns>proportional capacity for the item</returns>
        public int GetItemCapacity(Item item, float ratio)
        {
            switch (Mode)
            {
                case ItemStorageMode.Stacked:
                    var stackCount = Mathf.RoundToInt(ratio * StackCount);
                    return stackCount * Capacity * item.UnitSize;
                case ItemStorageMode.Free:
                    return int.MaxValue;
                case ItemStorageMode.ItemCapped:
                case ItemStorageMode.TotalItemCapped:
                    return Mathf.RoundToInt(ratio * Capacity);
                case ItemStorageMode.UnitCapped:
                case ItemStorageMode.TotalUnitCapped:
                    return Mathf.RoundToInt(ratio * Capacity) * item.UnitSize;
                case ItemStorageMode.Global:
                    return _globalStorage.GetItemCapacity(item, ratio);
                case ItemStorageMode.ItemSpecific:
                    return Mathf.RoundToInt(ItemCapacities.GetItemQuantity(item) * ratio);
                default:
                    return 0;
            }
        }
        /// <summary>
        /// calculates the total number of items in a category that may be stored
        /// </summary>
        /// <param name="itemCategory">item category in question</param>
        /// <returns>total capacity of items in the category</returns>
        public int GetItemCapacity(ItemCategory itemCategory) => GetItemCapacity(itemCategory, 1f);
        /// <summary>
        /// calculates the proportional number that may be stored of items in a category
        /// </summary>
        /// <param name="itemCategory">item category in question</param>
        /// <param name="ratio">how much of the capacity may be used (0-1)</param>
        /// <returns>proportional capacity for items in a category</returns>
        public int GetItemCapacity(ItemCategory itemCategory, float ratio)
        {
            if (itemCategory.Items.Length == 0)
                return 0;

            switch (Mode)
            {
                case ItemStorageMode.Stacked:
                case ItemStorageMode.Free:
                    return GetItemCapacity(itemCategory.Items[0], ratio);
                case ItemStorageMode.ItemCapped:
                case ItemStorageMode.UnitCapped:
                case ItemStorageMode.ItemSpecific:
                    return itemCategory.Items.Sum(i => GetItemCapacity(i, ratio));
                case ItemStorageMode.TotalItemCapped:
                    return Mathf.RoundToInt(ratio * Capacity);
                case ItemStorageMode.TotalUnitCapped:
                    return Mathf.RoundToInt(ratio * Capacity) * itemCategory.Items.First().UnitSize;
                case ItemStorageMode.Global:
                    return _globalStorage.GetItemCapacity(itemCategory);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// calculates the quantity of items that is above the ratio<br/>
        /// this can never be > 0  for ration 1 since items could use the entire capacity<br/>
        /// it would be half the capacity if the storage was full and ratio was 0.5
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ratio"></param>
        /// <returns>items stored above ratio</returns>
        public int GetItemsOverRatio(Item item, float ratio = 1f)
        {
            switch (Mode)
            {
                case ItemStorageMode.Stacked:
                    var stackCount = Mathf.RoundToInt(ratio * StackCount);
                    var maximum = stackCount * Capacity * item.UnitSize;
                    var current = GetItemQuantity(item);

                    return current - maximum;
                case ItemStorageMode.Free:
                    if (ratio == 0f)
                        return GetItemQuantity(item);
                    else
                        return 0;
                case ItemStorageMode.ItemCapped:
                    return _listItems.GetItemQuantity(item) - Mathf.RoundToInt(ratio * Capacity);
                case ItemStorageMode.UnitCapped:
                    return _listItems.GetItemQuantity(item) - Mathf.RoundToInt(ratio * Capacity) * item.UnitSize;
                case ItemStorageMode.Global:
                    return _globalStorage.GetItemsOverRatio(item, ratio);
                case ItemStorageMode.ItemSpecific:
                    return _listItems.GetItemQuantity(item) - GetItemCapacity(item, ratio);
                case ItemStorageMode.TotalItemCapped:
                    return _listItems.GetTotalQuantity() - Mathf.RoundToInt(ratio * Capacity);
                case ItemStorageMode.TotalUnitCapped:
                    return _listItems.GetTotalQuantity() - Mathf.RoundToInt(ratio * Capacity) * item.UnitSize;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// calculates how much space is left after removing stored items and reservations
        /// </summary>
        /// <returns>total space left</returns>
        public int GetItemCapacityRemaining() => GetItemCapacity() - GetItemQuantity() - _reservedCapacity.Sum(c => c.Quantity);
        /// <summary>
        /// calculates how many more of a particular item can be stored taking reservations into account
        /// </summary>
        /// <param name="item">item to check</param>
        /// <returns>remaining capacity after removing stored quantity and reservations</returns>
        public int GetItemCapacityRemaining(Item item) => GetItemCapacityRemaining(item, 1f);
        /// <summary>
        /// calculates how many more of a particular item can be stored taking reservations into account while limiting capacity to a specified ratio
        /// </summary>
        /// <param name="item">item to check</param>
        /// <param name="ratio">how much of the capacity may be used (0-1)</param>
        /// <returns>remaining proportional capacity after removing stored quantity and reservations</returns>
        public int GetItemCapacityRemaining(Item item, float ratio)
        {
            switch (Mode)
            {
                case ItemStorageMode.Stacked:
                    var stackCount = Mathf.RoundToInt(ratio * StackCount);
                    var usedStacks = Stacks.Where(s => s.Items != null && s.Items.Item == item).ToList();

                    var freeStacks = Math.Min(stackCount - usedStacks.Count, Stacks.Where(s => !s.HasItems).Count());

                    return freeStacks * Capacity * item.UnitSize + usedStacks.Sum(s => s.GetItemCapacityRemaining(item));
                case ItemStorageMode.Free:
                    return int.MaxValue;
                case ItemStorageMode.ItemCapped:
                    return Mathf.RoundToInt(ratio * Capacity) - _listItems.GetItemQuantity(item) - _reservedCapacity.GetItemQuantity(item);
                case ItemStorageMode.UnitCapped:
                    return Mathf.RoundToInt(ratio * Capacity) * item.UnitSize - _listItems.GetItemQuantity(item) - _reservedCapacity.GetItemQuantity(item);
                case ItemStorageMode.Global:
                    return _globalStorage.GetItemCapacityRemaining(item, ratio);
                case ItemStorageMode.ItemSpecific:
                    return GetItemCapacity(item, ratio) - _listItems.GetItemQuantity(item) - _reservedCapacity.GetItemQuantity(item);
                case ItemStorageMode.TotalItemCapped:
                    return Mathf.RoundToInt(ratio * Capacity) - _listItems.GetTotalQuantity() - _reservedCapacity.GetItemQuantity(item);
                case ItemStorageMode.TotalUnitCapped:
                    return Mathf.RoundToInt((ratio * Capacity - _listItems.GetTotalUnitQuantity() - _reservedCapacity.GetUnitQuantity(item)) * item.UnitSize);
                default:
                    return 0;
            }
        }
        /// <summary>
        /// calculates how many more items in a category can be stored while taking reservations into account
        /// </summary>
        /// <param name="itemCategory">the category of items to calculate</param>
        /// <returns>how many more items of the category can be stored</returns>
        public int GetItemCapacityRemaining(ItemCategory itemCategory) => GetItemCapacityRemaining(itemCategory, 1f);
        /// <summary>
        /// calculates how many more items in a category can be stored taking reservations into account while limiting capacity to a specified ratio
        /// </summary>
        /// <param name="itemCategory">item category to check</param>
        /// <param name="ratio">how much of the capacity may be used (0-1)</param>
        /// <returns>remaining proportional capacity after removing stored quantity and reservations</returns>
        public int GetItemCapacityRemaining(ItemCategory itemCategory, float ratio)
        {
            if (itemCategory.Items.Length == 0)
                return 0;

            switch (Mode)
            {
                case ItemStorageMode.Stacked:
                case ItemStorageMode.Free:
                case ItemStorageMode.TotalItemCapped:
                case ItemStorageMode.TotalUnitCapped:
                    return GetItemCapacityRemaining(itemCategory.Items[0], ratio);
                case ItemStorageMode.ItemCapped:
                case ItemStorageMode.UnitCapped:
                case ItemStorageMode.ItemSpecific:
                    return itemCategory.Items.Sum(i => GetItemCapacityRemaining(i, ratio));
                case ItemStorageMode.Global:
                    return _globalStorage.GetItemCapacityRemaining(itemCategory);
                default:
                    return 0;
            }
        }
        /// <summary>
        /// calculates how many more units(<see cref="Item.UnitSize"/>) of an items can be stored while subtracting reservations
        /// </summary>
        /// <param name="item">item to calculate</param>
        /// <returns>units of the items that can still be stored</returns>
        public float GetUnitCapacityRemaining(Item item)
        {
            return (float)GetItemCapacityRemaining(item) / item.UnitSize;
        }

        public ItemLevel GetItemLevel(Item item) => new ItemLevel(item, GetItemQuantity(item), GetItemCapacity(item));

        public bool FitsItems(IEnumerable<ItemQuantity> itemQuantities) => itemQuantities.All(FitsItems);
        public bool FitsItems(ItemQuantity itemQuantity) => FitsItems(itemQuantity.Item, itemQuantity.Quantity);
        /// <summary>
        /// checks whether a quantity fits into storage<br/>
        /// takes reservations into account
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        /// <returns>whether the items could be stored</returns>
        public bool FitsItems(Item item, int quantity)
        {
            return GetItemCapacityRemaining(item) >= quantity;
        }

        /// <summary>
        /// returns all the kinds of item in the storage
        /// </summary>
        /// <returns>item kinds stored</returns>
        public IEnumerable<Item> GetItems()
        {
            if (IsGlobalStorage)
            {
                foreach (var item in _globalStorage.GetItems())
                {
                    yield return item;
                }
            }
            else if (IsListStorage)
            {
                foreach (var item in _listItems.Select(i => i.Item))
                {
                    yield return item;
                }
            }
            else
            {
                List<Item> items = new List<Item>();
                foreach (var stack in Stacks)
                {
                    if (!stack.HasItems)
                        continue;
                    if (items.Contains(stack.Items.Item))
                        continue;

                    yield return stack.Items.Item;
                    items.Add(stack.Items.Item);
                }
            }
        }
        /// <summary>
        /// returns all the items in storage with their current quantity
        /// </summary>
        /// <returns>all items with quantities</returns>
        public IEnumerable<ItemQuantity> GetItemQuantities()
        {
            if (IsGlobalStorage)
            {
                return _globalStorage.GetItemQuantities();
            }
            else if (IsListStorage)
            {
                return _listItems;
            }
            else
            {
                Dictionary<Item, int> items = new Dictionary<Item, int>();
                foreach (var stack in Stacks)
                {
                    if (!stack.HasItems)
                        continue;

                    if (items.ContainsKey(stack.Items.Item))
                        items[stack.Items.Item] += stack.Items.Quantity;
                    else
                        items.Add(stack.Items.Item, stack.Items.Quantity);
                }

                foreach (var overflowItem in _overflowItems)
                {
                    if (items.ContainsKey(overflowItem.Item))
                        items[overflowItem.Item] += overflowItem.Quantity;
                    else
                        items.Add(overflowItem.Item, overflowItem.Quantity);
                }

                return items.Select(i => new ItemQuantity(i.Key, i.Value));
            }
        }

        /// <summary>
        /// moves any item into the other storage that will fit
        /// </summary>
        /// <param name="other">storage that receives the items</param>
        /// <returns>the item quantity that have been moved</returns>
        public List<ItemQuantity> MoveItemsTo(ItemStorage other) => MoveItemsTo(other, false);
        /// <summary>
        /// moves any item into the other storage
        /// </summary>
        /// <param name="other">storage that receives the items</param>
        /// <param name="overfill">whether exceeding the targets capacity is allowed</param>
        /// <returns>the item quantity that have been moved</returns>
        public List<ItemQuantity> MoveItemsTo(ItemStorage other, bool overfill)
        {
            List<ItemQuantity> moved = new List<ItemQuantity>();
            foreach (var items in GetItemQuantities().ToList())
            {
                int removed = items.Quantity - other.AddItems(items.Item, items.Quantity, overfill);
                RemoveItems(items.Item, removed);
                moved.AddQuantity(items.Item, removed);
            }
            return moved;
        }
        /// <summary>
        /// moves quantities of an items to another storage, may be limited by a maximum quantity
        /// </summary>
        /// <param name="other">the receiving storage</param>
        /// <param name="item">the items to move</param>
        /// <param name="maxQuantity">the maximum quantity that may be moved</param>
        /// <returns>the actual quantity moved</returns>
        public int MoveItemsTo(ItemStorage other, Item item, int maxQuantity = int.MaxValue, bool overfill = false)
        {
            var items = GetItemQuantities().FirstOrDefault(i => i.Item == item);
            if (items == null)
                return 0;

            int quantity = Math.Min(items.Quantity, maxQuantity);

            int remaining = other.AddItems(items.Item, quantity, overfill);
            RemoveItems(items.Item, quantity - remaining);

            return quantity;
        }

        public void AddItems(IEnumerable<ItemQuantity> itemQuantities, bool overfill = false)
        {
            foreach (var itemQuantity in itemQuantities)
            {
                AddItems(itemQuantity, overfill);
            }
        }
        public void RemoveItems(IEnumerable<ItemQuantity> itemQuantities)
        {
            if (itemQuantities == null)
                return;

            foreach (var itemQuantity in itemQuantities)
            {
                RemoveItems(itemQuantity);
            }
        }
        public void RemoveItems(IEnumerable<ItemCategoryQuantity> itemCategoryQuantities)
        {
            if (itemCategoryQuantities == null)
                return;

            foreach (var itemQuantity in itemCategoryQuantities)
            {
                RemoveItems(itemQuantity);
            }
        }

        /// <summary>
        /// adds items to that storage up to its capacity, remaining quantity is returned<br/>
        /// (adding 10 items to a storage that can only fit 4 more will return 6)
        /// </summary>
        /// <param name="itemQuantity">the quantity of items to add</param>
        /// <returns>remaining quantity that did not fit</returns>
        public int AddItems(ItemQuantity itemQuantity, bool overfill = false) => AddItems(itemQuantity.Item, itemQuantity.Quantity, overfill);
        public int AddItems(Item item, int quantity) => AddItems(item, quantity, false);
        /// <summary>
        /// adds items to that storage up to its capacity, remaining quantity is returned<br/>
        /// (adding 10 items to a storage that can only fit 4 more will return 6)
        /// </summary>
        /// <param name="item">the item to add</param>
        /// <param name="quantity">maximum quantity to add</param>
        /// <returns>remaining quantity that did not fit</returns>
        public int AddItems(Item item, int quantity, bool overfill)
        {
            if (IsGlobalStorage)
            {
                return _globalStorage.AddItems(item, quantity, overfill);
            }
            else if (IsListStorage)
            {
                int capacity = overfill ? int.MaxValue : getListItemCapacity(item);

                var items = _listItems.FirstOrDefault(i => i.Item == item);

                int stored = items?.Quantity ?? 0;
                int add = Mathf.Min(capacity - stored, quantity);

                if (items == null)
                    _listItems.Add(new ItemQuantity(item, add));
                else
                    items.Quantity += add;

                onChanged();

                return quantity - add;
            }
            else
            {
                foreach (var stack in Stacks)
                {
                    if (stack.HasItems && stack.Items.Item != item)
                        continue;//theres a different item in that stack

                    quantity = stack.AddQuantity(item, quantity);
                    if (quantity == 0)
                        break;
                }

                if (overfill && quantity > 0)
                {
                    _overflowItems.AddQuantity(item, quantity);
                    quantity = 0;
                }

                onChanged();

                return quantity;
            }
        }
        /// <summary>
        /// removes items from storage and returns the remaining quantity if not enough items were present<br/>
        /// (removing 4 items from a store that contains only 3 will return 1)
        /// </summary>
        /// <param name="itemQuantity">the quantity of item to remove</param>
        /// <returns>remaining quantity not removed</returns>
        public int RemoveItems(ItemQuantity itemQuantity) => RemoveItems(itemQuantity.Item, itemQuantity.Quantity);
        /// <summary>
        /// removes items from storage and returns the remaining quantity if not enough items were present<br/>
        /// (removing 4 items from a store that contains only 3 will return 1)
        /// </summary>
        /// <param name="item">the item to remove</param>
        /// <param name="quantity">the maximum quantity to remove</param>
        /// <returns>remaining quantity not removed</returns>
        public int RemoveItems(Item item, int quantity)
        {
            if (IsGlobalStorage)
            {
                return _globalStorage.RemoveItems(item, quantity);
            }
            else if (IsListStorage)
            {
                var items = _listItems.FirstOrDefault(i => i.Item == item);
                if (items == null)
                {
                    return quantity;
                }
                else
                {
                    if (items.Quantity > quantity)
                    {
                        items.Quantity -= quantity;

                        onChanged();

                        return 0;
                    }
                    else
                    {
                        _listItems.Remove(items);

                        onChanged();

                        return quantity - items.Quantity;
                    }
                }
            }
            else
            {
                quantity = _overflowItems.RemoveQuantity(item, quantity);

                if (quantity > 0)
                {
                    foreach (var stack in Stacks.OrderBy(s => s.FillDegree))
                    {
                        quantity = stack.SubtractQuantity(item, quantity);
                        if (quantity == 0)
                            break;
                    }
                }

                onChanged();

                return quantity;
            }
        }
        /// <summary>
        /// removes items from storage and returns the remaining quantity if not enough items were present<br/>
        /// (removing 4 items from a store that contains only 3 will return 1)
        /// </summary>
        /// <param name="itemCategoryQuantity">the itemCategoryQuantity to remove</param>
        /// <returns>remaining quantity not removed</returns>
        public int RemoveItems(ItemCategoryQuantity itemCategoryQuantity) => RemoveItems(itemCategoryQuantity.ItemCategory, itemCategoryQuantity.Quantity);
        /// <summary>
        /// removes items from storage and returns the remaining quantity if not enough items were present<br/>
        /// (removing 4 items from a store that contains only 3 will return 1)
        /// </summary>
        /// <param name="itemCategory">the itemCategory to remove</param>
        /// <param name="quantity">the maximum quantity to remove</param>
        /// <returns>remaining quantity not removed</returns>
        public int RemoveItems(ItemCategory itemCategory, int quantity)
        {
            foreach (var item in itemCategory.Items)
            {
                quantity = RemoveItems(item, quantity);
                if (quantity == 0)
                    break;
            }
            return quantity;
        }

        /// <summary>
        /// reserves capacity for an item that may be delivered in the future<br/>
        /// makes sure that capacity is still available when the item arrives
        /// </summary>
        /// <param name="item">the item to reserve</param>
        /// <param name="quantity">quantity of the item to reserve</param>
        public void ReserveCapacity(Item item, int quantity)
        {
            _reservedCapacity.AddQuantity(item, quantity);
        }
        /// <summary>
        /// removes a reservation made by <see cref="ReserveCapacity(Item, int)"/><br/>
        /// should be called when the items arrives or if the delivery got interrupted
        /// </summary>
        /// <param name="item">the item to unreseve</param>
        /// <param name="quantity">quantity of the item to unreserve</param>
        public void UnreserveCapacity(Item item, int quantity)
        {
            _reservedCapacity.RemoveQuantity(item, quantity);
        }
        /// <summary>
        /// checks how much capacity has been reserved for an item
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <returns>the capacity that has been reserved for the given item</returns>
        public int GetReservedCapacity(Item item) => _reservedCapacity.GetItemQuantity(item);

        /// <summary>
        /// reserves a quantity of an item so it can be collected in the future<br/>
        /// makes sure the items dont get removed from somebody else until it is acutally removed
        /// </summary>
        /// <param name="item">the item to reserve quantity for</param>
        /// <param name="quantity">the quantity to reserve</param>
        public void ReserveQuantity(Item item, int quantity)
        {
            _reservedQuantity.AddQuantity(item, quantity);
        }
        /// <summary>
        /// removes a reservation made by <see cref="ReserveQuantity(Item, int)"/><br/>
        /// should be called when the items are collected or if the pickup was interrupted
        /// </summary>
        /// <param name="item">the item to unreserve quantity for</param>
        /// <param name="quantity">the quantity to unreserve</param>
        public void UnreserveQuantity(Item item, int quantity)
        {
            _reservedQuantity.RemoveQuantity(item, quantity);
        }
        /// <summary>
        /// checks how much quantity has been reserved of an item
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <returns>the quantity that has been reserved of the item</returns>
        public int GetReservedQuantity(Item item) => _reservedQuantity.GetItemQuantity(item);

        /// <summary>
        /// text that can be displayed for easy debugging<br/>
        /// prints out all the item quantities stored
        /// </summary>
        /// <returns></returns>
        public string GetDebugText()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var items in GetItemQuantities())
            {
                sb.AppendLine($"{items.Item.Key}: {items.Quantity}");
            }

            return sb.ToString();
        }
        public string GetItemNames()
        {
            return string.Join(", ", GetItems().Select(i => i.Name));
        }

        private void setupStacks()
        {
            int count = IsStackedStorage ? StackCount : 0;

            _stacks = new ItemStack[count];
            for (int i = 0; i < count; i++)
            {
                _stacks[i] = new ItemStack() { UnitCapacity = Capacity };
            }
        }

        private int getListItemCapacity(Item item)
        {
            switch (Mode)
            {
                case ItemStorageMode.ItemCapped:
                    return Capacity;
                case ItemStorageMode.UnitCapped:
                    return Capacity * item.UnitSize;
                case ItemStorageMode.ItemSpecific:
                    return ItemCapacities.GetItemQuantity(item);
                default:
                    return int.MaxValue;
            }
        }

        private void onChanged() => Changed?.Invoke(this);

        private static ItemQuantity.ItemQuantityData[] getData(ItemStack[] stacks)
        {
            if (stacks == null)
                return null;
            return stacks.Select(s => s.GetData()).ToArray();
        }
        private static ItemQuantity.ItemQuantityData[] getData(List<ItemQuantity> items)
        {
            if (items == null)
                return null;
            return items.Select(i => i.GetData()).ToArray();
        }
        private static void getStacks(ItemQuantity.ItemQuantityData[] datas, ItemStack[] stacks)
        {
            if (datas == null)
                return;

            for (int i = 0; i < stacks.Length; i++)
            {
                var data = datas.ElementAtOrDefault(i);
                if (data == null || string.IsNullOrWhiteSpace(data.Key))
                    continue;
                stacks[i].SetData(data);
            }
        }
        private static List<ItemQuantity> getItems(ItemQuantity.ItemQuantityData[] datas)
        {
            if (datas == null)
                return null;
            return datas.Select(d => new ItemQuantity(Dependencies.Get<IKeyedSet<Item>>().GetObject(d.Key), d.Quantity)).ToList();
        }

        #region Saving
        [Serializable]
        public class ItemStorageData
        {
            public ItemQuantity.ItemQuantityData[] Stacked;
            public ItemQuantity.ItemQuantityData[] Free;
            public ItemQuantity.ItemQuantityData[] ReservedCapacity;
            public ItemQuantity.ItemQuantityData[] ReservedQuantity;
        }

        public ItemStorageData SaveData()
        {
            return new ItemStorageData()
            {
                Stacked = getData(_stacks),
                Free = getData(_listItems),
                ReservedCapacity = getData(_reservedCapacity),
                ReservedQuantity = getData(_reservedQuantity)
            };
        }
        public void LoadData(ItemStorageData data)
        {
            getStacks(data.Stacked, Stacks);
            _listItems = getItems(data.Free);
            _reservedCapacity = getItems(data.ReservedCapacity);
            _reservedQuantity = getItems(data.ReservedQuantity);
        }
        #endregion
    }
}