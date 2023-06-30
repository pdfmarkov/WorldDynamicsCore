using NUnit.Framework;
using UnityEngine;

namespace CityBuilderCore.Tests
{
    public class ItemStorageTesting
    {
        private class GlobalStorageTester : IGlobalStorage
        {
            public ItemStorage ItemStorage { get; set; }

            public ItemStorage Items => ItemStorage;
        }

        [Test]
        public void ReserveQuantity()
        {
            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.Free
            };

            var item = ScriptableObject.CreateInstance<Item>();

            storage.AddItems(item, 10);

            Assert.AreEqual(10, storage.GetItemQuantityRemaining(item));
            storage.ReserveQuantity(item, 5);
            Assert.AreEqual(5, storage.GetItemQuantityRemaining(item));
            storage.UnreserveQuantity(item, 5);
            Assert.AreEqual(10, storage.GetItemQuantityRemaining(item));
        }
        [Test]
        public void ReserveCapacity()
        {
            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.ItemCapped,
                Capacity = 10
            };

            var item = ScriptableObject.CreateInstance<Item>();

            Assert.AreEqual(10, storage.GetItemCapacityRemaining(item));
            storage.ReserveCapacity(item, 5);
            Assert.AreEqual(5, storage.GetItemCapacityRemaining(item));
            storage.UnreserveCapacity(item, 5);
            Assert.AreEqual(10, storage.GetItemCapacityRemaining(item));
        }

        [Test]
        public void MoveItems()
        {
            var storageA = new ItemStorage()
            {
                Mode = ItemStorageMode.ItemCapped,
                Capacity = 100
            };
            var storageB = new ItemStorage()
            {
                Mode = ItemStorageMode.ItemCapped,
                Capacity = 10
            };

            var item = ScriptableObject.CreateInstance<Item>();

            storageA.AddItems(item, 50);
            storageA.MoveItemsTo(storageB);

            Assert.AreEqual(40, storageA.GetItemQuantity(item));
            Assert.AreEqual(10, storageB.GetItemQuantity(item));

            storageB.MoveItemsTo(storageA);

            Assert.AreEqual(50, storageA.GetItemQuantity(item));
            Assert.AreEqual(0, storageB.GetItemQuantity(item));

            storageA.MoveItemsTo(storageB, item, 5);

            Assert.AreEqual(45, storageA.GetItemQuantity(item));
            Assert.AreEqual(5, storageB.GetItemQuantity(item));
        }

        [Test]
        public void CheckStacked()
        {
            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.Stacked,
                StackCount = 4,
                Capacity = 10
            };

            var itemA = ScriptableObject.CreateInstance<Item>();
            var itemB = ScriptableObject.CreateInstance<Item>();

            storage.AddItems(itemA, 25);
            storage.AddItems(itemB, 5);

            Assert.AreEqual(itemA, storage.Stacks[0].Items.Item);
            Assert.AreEqual(itemA, storage.Stacks[1].Items.Item);
            Assert.AreEqual(itemA, storage.Stacks[2].Items.Item);
            Assert.AreEqual(itemB, storage.Stacks[3].Items.Item);

            Assert.AreEqual(10, storage.Stacks[0].Items.Quantity);
            Assert.AreEqual(10, storage.Stacks[1].Items.Quantity);
            Assert.AreEqual(5, storage.Stacks[2].Items.Quantity);
            Assert.AreEqual(5, storage.Stacks[3].Items.Quantity);

            Assert.AreEqual(5, storage.GetItemCapacityRemaining(itemA));
            Assert.AreEqual(5, storage.GetItemCapacityRemaining(itemB));

            Assert.AreEqual(25, storage.GetItemQuantity(itemA));
            Assert.AreEqual(5, storage.GetItemQuantity(itemB));
        }

        [Test]
        public void CheckItemCapped()
        {
            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.ItemCapped,
                Capacity = 10
            };

            var itemA = ScriptableObject.CreateInstance<Item>();
            var itemB = ScriptableObject.CreateInstance<Item>();

            storage.AddItems(itemA, 5);
            storage.AddItems(itemB, 10);

            Assert.AreEqual(5, storage.GetItemCapacityRemaining(itemA));
            Assert.AreEqual(0, storage.GetItemCapacityRemaining(itemB));

            Assert.AreEqual(5, storage.GetItemQuantity(itemA));
            Assert.AreEqual(10, storage.GetItemQuantity(itemB));

            Assert.AreEqual(0, storage.GetItemCapacityRemaining(itemA, 0.5f));
            Assert.AreEqual(0, storage.GetItemsOverRatio(itemA, 0.5f));
            Assert.AreEqual(5, storage.GetItemsOverRatio(itemA, 0.0f));
        }
        [Test]
        public void CheckTotalItemCapped()
        {
            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.TotalItemCapped,
                Capacity = 20
            };

            var itemA = ScriptableObject.CreateInstance<Item>();
            var itemB = ScriptableObject.CreateInstance<Item>();

            storage.AddItems(itemA, 5);
            storage.AddItems(itemB, 10);

            Assert.AreEqual(5, storage.GetItemCapacityRemaining(itemA));
            Assert.AreEqual(5, storage.GetItemCapacityRemaining(itemB));

            Assert.AreEqual(5, storage.GetItemQuantity(itemA));
            Assert.AreEqual(10, storage.GetItemQuantity(itemB));
        }

        [Test]
        public void CheckUnitCapped()
        {
            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.UnitCapped,
                Capacity = 10
            };

            var itemA = ScriptableObject.CreateInstance<Item>();
            var itemB = ScriptableObject.CreateInstance<Item>();

            itemA.UnitSize = 10;
            itemB.UnitSize = 1;

            storage.AddItems(itemA, 50);
            storage.AddItems(itemB, 10);

            Assert.AreEqual(50, storage.GetItemCapacityRemaining(itemA));
            Assert.AreEqual(0, storage.GetItemCapacityRemaining(itemB));

            Assert.AreEqual(50, storage.GetItemQuantity(itemA));
            Assert.AreEqual(10, storage.GetItemQuantity(itemB));
        }
        [Test]
        public void CheckTotalUnitCapped()
        {
            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.TotalUnitCapped,
                Capacity = 10
            };

            var itemA = ScriptableObject.CreateInstance<Item>();
            var itemB = ScriptableObject.CreateInstance<Item>();

            itemA.UnitSize = 10;
            itemB.UnitSize = 1;

            storage.AddItems(itemA, 30);
            storage.AddItems(itemB, 2);

            Assert.AreEqual(50, storage.GetItemCapacityRemaining(itemA));
            Assert.AreEqual(5, storage.GetItemCapacityRemaining(itemB));

            Assert.AreEqual(30, storage.GetItemQuantity(itemA));
            Assert.AreEqual(2, storage.GetItemQuantity(itemB));
        }

        [Test]
        public void CheckGlobal()
        {
            Dependencies.Clear();

            var globalStorage = new GlobalStorageTester()
            {
                ItemStorage = new ItemStorage()
                {
                    Mode = ItemStorageMode.Free
                }
            };
            Dependencies.Register<IGlobalStorage>(globalStorage);

            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.Global
            };

            var item = ScriptableObject.CreateInstance<Item>();

            storage.AddItems(item, 5);
            globalStorage.ItemStorage.AddItems(item, 10);

            Assert.AreEqual(15, storage.GetItemQuantity(item));
            Assert.AreEqual(15, globalStorage.ItemStorage.GetItemQuantity(item));

            Dependencies.Clear();
        }

        [Test]
        public void CheckItemSpecific()
        {
            var itemA = ScriptableObject.CreateInstance<Item>();
            var itemB = ScriptableObject.CreateInstance<Item>();

            var storage = new ItemStorage()
            {
                Mode = ItemStorageMode.ItemSpecific,
                ItemCapacities = new ItemQuantity[]
                {
                    new ItemQuantity(itemA,10),
                    new ItemQuantity(itemB,5)
                }
            };

            storage.AddItems(itemA, 5);
            storage.AddItems(itemB, 1);

            Assert.AreEqual(5, storage.GetItemQuantity(itemA));
            Assert.AreEqual(1, storage.GetItemQuantity(itemB));

            Assert.AreEqual(5, storage.GetItemCapacityRemaining(itemA));
            Assert.AreEqual(4, storage.GetItemCapacityRemaining(itemB));
        }
    }
}
