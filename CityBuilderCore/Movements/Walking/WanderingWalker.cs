using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// walker that randomly wanders in any direction one unit at a time for a set number of steps
    /// </summary>
    public class WanderingWalker : Walker
    {
        [Tooltip("how many steps the wanderer will take before vanishing")]
        public int Range = 64;

        private int _steps;

        public override void Initialize(BuildingReference home, Vector2Int start)
        {
            base.Initialize(home, start);

            _steps = 0;

            wanderNext();
        }

        private void wanderNext()
        {
            _steps++;
            if (_steps > Range)
                onFinished();

            wander(wanderNext);
        }

        #region Saving
        [Serializable]
        public class WanderingWalkerData
        {
            public WalkerData WalkerData;
            public int Steps;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new WanderingWalkerData()
            {
                WalkerData = savewalkerData(),
                Steps = _steps
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<WanderingWalkerData>(json);

            loadWalkerData(data.WalkerData);

            _steps = data.Steps;

            continueWander(wanderNext);
        }
        #endregion
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class ManualWanderingWalkerSpawner : ManualWalkerSpawner<WanderingWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class CyclicWanderingWalkerSpawner : CyclicWalkerSpawner<WanderingWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class PooledWanderingWalkerSpawner : PooledWalkerSpawner<WanderingWalker> { }
}