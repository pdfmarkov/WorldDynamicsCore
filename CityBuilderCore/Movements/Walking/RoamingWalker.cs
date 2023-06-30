﻿using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// walker that semi randomly walks about and then returns home<br/>
    /// has a memory so he diversifies his route(chooses different direction every time he leaves building)
    /// </summary>
    public class RoamingWalker : Walker
    {
        public enum RoamingWalkerState
        {
            Inactive = 0,
            Roaming = 1,
            Waiting = 2,
            Returning = 3
        }
        
        [Tooltip("how many points the walker will memorize try to avoid")]
        public int Memory = 64;
        [Tooltip("how many steps the walker roams before returning")]
        public int Range = 16;
        [Tooltip("whether the walker walks back home after roaming or just vanishes")]
        public bool ReturnHome = true;

        protected RoamingWalkerState _state = RoamingWalkerState.Inactive;

        public override void Initialize(BuildingReference home, Vector2Int start)
        {
            base.Initialize(home, start);

            _state = RoamingWalkerState.Roaming;
            roam(Memory, Range, onRoamFinished);
        }

        protected virtual void onRoamFinished()
        {
            if (ReturnHome)
            {
                _state = RoamingWalkerState.Waiting;

                tryWalk(() => PathHelper.FindPath(_current, _start, PathType, PathTag), planned: () => _state = RoamingWalkerState.Returning);
            }
            else
            {
                onFinished();
            }
        }

        protected override void onFinished()
        {
            _state = RoamingWalkerState.Inactive;
            base.onFinished();
        }

        public override string GetDescription()
        {
            return getDescription((int)_state, Home.Instance.GetName());
        }

        #region Saving
        [Serializable]
        public class RoamingWalkerData
        {
            public WalkerData WalkerData;
            public int State;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new RoamingWalkerData()
            {
                WalkerData = savewalkerData(),
                State = (int)_state
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<RoamingWalkerData>(json);

            loadWalkerData(data.WalkerData);

            _state = (RoamingWalkerState)data.State;

            switch (_state)
            {
                case RoamingWalkerState.Roaming:
                    continueRoam(Memory, Range, onRoamFinished);
                    break;
                case RoamingWalkerState.Waiting:
                    onRoamFinished();
                    break;
                case RoamingWalkerState.Returning:
                    continueWalk();
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class ManualRoamingWalkerSpawner : ManualWalkerSpawner<RoamingWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class CyclicRoamingWalkerSpawner : CyclicWalkerSpawner<RoamingWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class PooledRoamingWalkerSpawner : PooledWalkerSpawner<RoamingWalker> { }
}