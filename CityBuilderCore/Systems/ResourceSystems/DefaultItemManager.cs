using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// default implementation for the resource systems<br/>
    /// <para>
    /// manages global items as well as finding the right givers, receivers and dispensers for items
    /// </para>
    /// </summary>
    public class DefaultItemManager : MonoBehaviour, IGlobalStorage, IGiverPathfinder, IReceiverPathfinder, IItemsDispenserManager
    {
        [Tooltip("global item storage")]
        public ItemStorage ItemStorage;
        [Tooltip("items added to global storage when the game starts")]
        public ItemQuantity[] StartItems;

        public ItemStorage Items => ItemStorage;

        private List<IItemsDispenser> _dispensers = new List<IItemsDispenser>();

        protected virtual void Awake()
        {
            Dependencies.Register<IGlobalStorage>(this);
            Dependencies.Register<IGiverPathfinder>(this);
            Dependencies.Register<IReceiverPathfinder>(this);
            Dependencies.Register<IItemsDispenserManager>(this);
        }

        protected virtual void Start()
        {
            StartItems.ForEach(i => ItemStorage.AddItems(i.Item, i.Quantity));
        }

        public void Add(IItemsDispenser dispenser)
        {
            _dispensers.Add(dispenser);
        }
        public void Remove(IItemsDispenser dispenser)
        {
            _dispensers.Remove(dispenser);
        }

        public IItemsDispenser GetDispenser(string key, Vector3 position, float maxDistance)
        {
            return _dispensers
                .Where(d => d.Key == key)
                .Select(d => Tuple.Create(d, Vector3.Distance(d.Position, position)))
                .Where(d => d.Item2 < maxDistance)
                .OrderBy(d => d.Item2)
                .FirstOrDefault()?.Item1;
        }
        public bool HasDispenser(string key, Vector3 position, float maxDistance)
        {
            return _dispensers
                .Where(d => d.Key == key)
                .Select(d => Tuple.Create(d, Vector3.Distance(d.Position, position)))
                .Where(d => d.Item2 < maxDistance)
                .Any();
        }

        public BuildingComponentPath<IItemGiver> GetGiverPath(IBuilding building, Vector2Int? currentPoint, ItemQuantity items, float maxDistance, PathType pathType, object pathTag = null)
        {
            if (items == null)
                return null;

            List<IItemGiver> givers = new List<IItemGiver>();
            foreach (var itemGiver in Dependencies.Get<IBuildingManager>().GetBuildingTraits<IItemGiver>())
            {
                if (!isValid(building, items, itemGiver))
                    continue;//dont deliver to self

                Vector3 currentPosition;
                if (currentPoint.HasValue)
                    currentPosition = Dependencies.Get<IGridPositions>().GetWorldPosition(currentPoint.Value);
                else
                    currentPosition = building.WorldCenter;

                var distance = Vector2.Distance(itemGiver.Building.WorldCenter, currentPosition);
                if (distance > maxDistance)
                    continue;//too far away

                if (itemGiver.GetGiveQuantity(items.Item) < items.Quantity)
                    continue;//does not have the items

                givers.Add(itemGiver);
            }

            if (givers.Count == 1)
            {
                var path = PathHelper.FindPath(building, currentPoint, givers[0].Building, pathType, pathTag);
                if (path == null)
                    return null;

                return new BuildingComponentPath<IItemGiver>(givers[0].Reference, path);
            }
            else if (givers.Count > 1)
            {
                IItemGiver currentGiver = null;
                WalkingPath currentPath = null;

                foreach (var giver in givers)
                {
                    var path = PathHelper.FindPath(building, currentPoint, giver.Building, pathType, pathTag);
                    if (path == null)
                        continue;

                    var score = maxDistance - path.GetDistance();
                    if (currentGiver == null || isMoreImportant(items, currentGiver, currentPath, giver, path))
                    {
                        currentGiver = giver;
                        currentPath = path;
                    }
                }

                if (currentGiver != null)
                    return new BuildingComponentPath<IItemGiver>(currentGiver.Reference, currentPath);
            }

            return null;
        }

        public BuildingComponentPath<IItemReceiver> GetReceiverPath(IBuilding building, Vector2Int? currentPoint, ItemQuantity items, float maxDistance, PathType pathType, object pathTag = null, int currentPriority = 0)
        {
            if (items == null)
                return null;

            List<IItemReceiver> receivers = new List<IItemReceiver>();
            foreach (var itemReceiver in Dependencies.Get<IBuildingManager>().GetBuildingTraits<IItemReceiver>())
            {
                if (!isValid(building, items, itemReceiver, currentPriority))
                    continue;

                if (!itemReceiver.GetReceiveItems().Contains(items.Item))
                    continue;

                Vector3 currentPosition;
                if (currentPoint.HasValue)
                    currentPosition = Dependencies.Get<IGridPositions>().GetWorldPosition(currentPoint.Value);
                else
                    currentPosition = building.WorldCenter;

                var distance = Vector2.Distance(itemReceiver.Building.WorldCenter, currentPosition);
                if (distance > maxDistance)
                    continue;//too far away

                var missing = itemReceiver.GetReceiveCapacity(items.Item) / (float)items.Item.UnitSize;
                if (missing < 1f)
                    continue;//not enough space remaining

                receivers.Add(itemReceiver);
            }

            if (receivers.Count == 1)
            {
                var path = PathHelper.FindPath(building, currentPoint, receivers[0].Building, pathType, pathTag);
                if (path == null)
                    return null;

                return new BuildingComponentPath<IItemReceiver>(receivers[0].Reference, path);
            }
            else if (receivers.Count > 1)
            {
                IItemReceiver currentReceiver = null;
                WalkingPath currentPath = null;

                foreach (var receiver in receivers)
                {
                    var path = PathHelper.FindPath(building, currentPoint, receiver.Building, pathType, pathTag);
                    if (path == null)
                        continue;

                    var unitsMissing = (float)receiver.GetReceiveCapacity(items.Item) / items.Item.UnitSize;
                    var score = unitsMissing * 2f - path.GetDistance();

                    if (currentReceiver == null || isMoreImportant(items, currentReceiver, currentPath, receiver, path, currentPriority))
                    {
                        currentReceiver = receiver;
                        currentPath = path;
                    }
                }

                if (currentReceiver != null)
                    return new BuildingComponentPath<IItemReceiver>(currentReceiver.Reference, currentPath);
            }

            return null;
        }

        protected virtual bool isValid(IBuilding building, ItemQuantity items, IItemGiver giver)
        {
            if (giver.Building == building)
                return false;//dont deliver to self

            return true;
        }
        protected virtual bool isValid(IBuilding building, ItemQuantity items, IItemReceiver receiver, int currentPriority = 0)
        {
            if (receiver.Building == building)
                return false;//dont deliver to self

            if (receiver.Priority <= currentPriority)
                return false;//receiver has same or lower priority then the current storage

            return true;
        }

        protected virtual bool isMoreImportant(ItemQuantity items, IItemGiver giverA, WalkingPath pathA, IItemGiver giverB, WalkingPath pathB)
        {
            return pathB.GetDistance() < pathA.GetDistance();
        }
        protected virtual bool isMoreImportant(ItemQuantity items, IItemReceiver receiverA, WalkingPath pathA, IItemReceiver receiverB, WalkingPath pathB, int currentPriority = 0)
        {
            if (receiverB.Priority > receiverA.Priority)
                return true;

            var missingA = (float)receiverA.GetReceiveCapacity(items.Item) / items.Item.UnitSize;
            var missingB = (float)receiverB.GetReceiveCapacity(items.Item) / items.Item.UnitSize;

            var scoreA = missingA * 2f - pathA.GetDistance();
            var scoreB = missingB * 2f - pathB.GetDistance();

            return scoreB > scoreA;
        }
    }
}