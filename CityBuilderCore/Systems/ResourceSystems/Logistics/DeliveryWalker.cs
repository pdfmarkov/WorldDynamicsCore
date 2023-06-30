using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// walker that tries to find an <see cref="IItemReceiver"/> for the items it is given and deliver them<br/>
    /// if it cant find a receiver it will idle until the <see cref="Walker.MaxWait"/> runs out and the items perish
    /// </summary>
    public class DeliveryWalker : Walker
    {
        public enum DeliveryWalkerState
        {
            Inactive = 0,
            WaitingDelivery = 1,
            Delivering = 2,
            WaitingReturn = 3,
            Returning = 4,
        }

        [Tooltip("storage the walker stores the items that it tries to deliver")]
        public ItemStorage Storage;
        [Tooltip("maximum distance to receiver as the crow flies")]
        public float MaxDistance = 100;
        [Tooltip("whether the walker has to go back home after delivering its items before it is available again")]
        public bool ReturnHome = true;

        public override ItemStorage ItemStorage => Storage;

        private DeliveryWalkerState _state = DeliveryWalkerState.Inactive;
        private BuildingComponentReference<IItemReceiver> _receiver;

        public override void Initialize(BuildingReference home, Vector2Int start)
        {
            base.Initialize(home, start);

            _state = DeliveryWalkerState.Inactive;
        }

        public void StartDelivery(ItemStorage storage)
        {
            storage.MoveItemsTo(Storage);

            tryDeliver();
        }
        public void StartDelivery(ItemStorage storage, Item item)
        {
            storage.MoveItemsTo(Storage, item);

            tryDeliver();
        }

        private void deliver()
        {
            _start = _current;
            _receiver.Instance.ReceiveAll(Storage);

            if (Storage.HasItems())
            {
                tryDeliver(_current);
            }
            else
            {
                if (ReturnHome)
                {
                    tryReturn();
                }
                else
                {
                    onFinished();
                }
            }
        }

        private void tryDeliver(Vector2Int? position = null)
        {
            _state = DeliveryWalkerState.WaitingDelivery;
            tryWalk(() =>
             {
                 var receiverPath = Dependencies.Get<IReceiverPathfinder>().GetReceiverPath(Home.Instance, position, Storage.GetItemQuantities().FirstOrDefault(), MaxDistance, base.PathType, base.PathTag);
                 if (receiverPath == null)
                     return null;

                 _receiver = receiverPath.Component;
                 return receiverPath.Path;
             },
              planned: () => _state = DeliveryWalkerState.Delivering,
              finished: deliver, canceled: onFinished);
        }

        private void tryReturn()
        {
            _state = DeliveryWalkerState.WaitingReturn;
            tryWalk(Home.Instance, planned: () => _state = DeliveryWalkerState.Returning, finished: onFinished);
        }

        protected override void onFinished()
        {
            _state = DeliveryWalkerState.Inactive;
            base.onFinished();
        }

        public override string GetDescription()
        {
            List<object> parameters = new List<object>();

            parameters.Add(Home.Instance.GetName());

            switch (_state)
            {
                case DeliveryWalkerState.WaitingDelivery:
                    parameters.Add(Storage.GetItemNames());
                    break;
                case DeliveryWalkerState.Delivering:
                    parameters.Add(Storage.GetItemNames());
                    parameters.Add(_receiver.Instance.Building.GetName());
                    break;
            }

            return getDescription((int)_state, parameters.ToArray());
        }

        public override string GetDebugText() => Storage.GetDebugText();

        #region Saving
        [Serializable]
        public class DeliveryWalkerData
        {
            public WalkerData WalkerData;
            public int State;
            public BuildingComponentReferenceData Receiver;
            public ItemStorage.ItemStorageData Storage;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new DeliveryWalkerData()
            {
                WalkerData = savewalkerData(),
                Storage = Storage.SaveData(),
                State = (int)_state,
                Receiver = _receiver?.GetData()
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<DeliveryWalkerData>(json);

            loadWalkerData(data.WalkerData);

            Storage.LoadData(data.Storage);

            _state = (DeliveryWalkerState)data.State;
            _receiver = data.Receiver.GetReference<IItemReceiver>();

            switch (_state)
            {
                case DeliveryWalkerState.WaitingDelivery:
                    tryDeliver(_start);
                    break;
                case DeliveryWalkerState.Delivering:
                    continueWalk(deliver);
                    break;
                case DeliveryWalkerState.WaitingReturn:
                    tryReturn();
                    break;
                case DeliveryWalkerState.Returning:
                    continueWalk();
                    break;
                default:
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class ManualDeliveryWalkerSpawner : ManualWalkerSpawner<DeliveryWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class CyclicDeliveryWalkerSpawner : CyclicWalkerSpawner<DeliveryWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class PooledDeliveryWalkerSpawner : PooledWalkerSpawner<DeliveryWalker> { }
}