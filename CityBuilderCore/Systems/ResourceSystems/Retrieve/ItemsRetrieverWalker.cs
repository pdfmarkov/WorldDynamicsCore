using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// walker that retrieves items from <see cref="IItemsDispenser"/> filtered by their key<br/>
    /// does not adjust the path when the dispenser moves<br/>
    /// if it arrives at its destination and the dispenser is out of <see cref="RetrieveDistance"/> it will move again to get close enough
    /// </summary>
    public class ItemsRetrieverWalker : Walker, IItemOwner
    {
        public enum ItemsRetrieverWalkerState
        {
            Idle = 0,
            Approaching = 10,
            Retrieving = 20,
            Returning = 30
        }

        [Tooltip("key of the dispensert this walker targets")]
        public string DispenserKey;
        [Tooltip("maximum distance from home to dispenser")]
        public float MaxDistance = 100;
        [Tooltip("maximum distance for the retriever use a dispenser")]
        public float RetrieveDistance = 1;
        [Tooltip("how long the retriever waits after dispensing")]
        public float RetrieveTime;
        [Tooltip("how fast the walker moves when approaching, regular speed is used returning")]
        public float ApproachSpeed;
        [Tooltip("storage used to store items while carrying them home from the dispenser")]
        public ItemStorage Storage;
        [Tooltip(@"fires whenever the state of the walker changes, useful for animation
Idle        0
Approaching 10
Retrieving  20
Returning   30")]
        public IntEvent IsStateChanged;

        public override ItemStorage ItemStorage => Storage;
        public IItemContainer ItemContainer => Storage;
        public override float Speed => _state == ItemsRetrieverWalkerState.Approaching && ApproachSpeed > 0 ? ApproachSpeed : base.Speed;

        private ItemsRetrieverWalkerState _state;

        public override void Initialize(BuildingReference home, Vector2Int start)
        {
            base.Initialize(home, start);

            _state = ItemsRetrieverWalkerState.Approaching;
            IsStateChanged?.Invoke((int)_state);
        }

        public void StartRetrieving(IItemsDispenser dispenser = null)
        {
            retrieve(dispenser, true);
        }

        private void retrieve() => retrieve(null);
        private void retrieve(IItemsDispenser dispenser, bool force = false)
        {
            var worldPosition = Dependencies.Get<IGridPositions>().GetWorldPosition(_current);

            if (!(dispenser as UnityEngine.Object))
                dispenser = Dependencies.Get<IItemsDispenserManager>().GetDispenser(DispenserKey, worldPosition, MaxDistance);

            if (!(dispenser as UnityEngine.Object))
            {
                onFinished();
            }
            else
            {
                if (!force && Vector3.Distance(dispenser.Position, worldPosition) < RetrieveDistance)
                {
                    dispense(dispenser);
                }
                else
                {
                    var path = PathHelper.FindPath(_current, Dependencies.Get<IGridPositions>().GetGridPosition(dispenser.Position), PathType, PathTag);
                    if (path == null)
                        onFinished();
                    else if (path.Length <= 1)
                        dispense(dispenser);
                    else
                        walk(path, 0f, retrieve);
                }
            }
        }

        private void dispense(IItemsDispenser dispenser)
        {
            var items = dispenser.Dispense();
            Storage.AddItems(items.Item, items.Quantity);

            _state = ItemsRetrieverWalkerState.Retrieving;
            IsStateChanged?.Invoke((int)_state);

            wait(returnHome, RetrieveTime);
        }

        private void returnHome()
        {
            _state = ItemsRetrieverWalkerState.Returning;
            IsStateChanged?.Invoke((int)_state);

            var path = PathHelper.FindPath(_current, Home.Instance, PathType, PathTag);
            if (path == null)
                onFinished();
            else
                walk(path, 0f);
        }

        public override string GetDebugText() => Storage.GetDebugText();

        #region Saving
        [Serializable]
        public class ItemsRetrieverWalkerData
        {
            public WalkerData WalkerData;
            public int State;
            public ItemStorage.ItemStorageData Storage;
            public ItemQuantity.ItemQuantityData Order;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new ItemsRetrieverWalkerData()
            {
                WalkerData = savewalkerData(),
                Storage = Storage.SaveData(),
                State = (int)_state,
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<ItemsRetrieverWalkerData>(json);

            loadWalkerData(data.WalkerData);

            Storage.LoadData(data.Storage);

            _state = (ItemsRetrieverWalkerState)data.State;

            switch (_state)
            {
                case ItemsRetrieverWalkerState.Approaching:
                    continueWalk(retrieve);
                    break;
                case ItemsRetrieverWalkerState.Retrieving:
                    continueWait(returnHome);
                    break;
                case ItemsRetrieverWalkerState.Returning:
                    continueWalk();
                    break;
                default:
                    break;
            }

            IsStateChanged?.Invoke((int)_state);
        }
        #endregion
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class ManualItemsRetrieverWalkerSpawner : ManualWalkerSpawner<ItemsRetrieverWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class CyclicItemsRetrieverWalkerSpawner : CyclicWalkerSpawner<ItemsRetrieverWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class PooledItemsRetrieverWalkerSpawner : PooledWalkerSpawner<ItemsRetrieverWalker> { }
}