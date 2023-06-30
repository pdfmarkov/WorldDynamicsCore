using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// provides a fixed quantity of workers periodically as long as the buildings efficiency is working<br/>
    /// workers need to return and wait out their cooldown before being deployed again
    /// </summary>
    public class PooledWorkerProviderComponent : BuildingComponent
    {
        public override string Key => "PWP";

        [Tooltip("spawner for configuring and managing the worker walkers")]
        public PooledWorkerWalkerSpawner WorkerWalkers;

        private void Awake()
        {
            WorkerWalkers.Initialize(Building, workerLeaving);
        }
        private void Update()
        {
            if (Building.IsWorking)
                WorkerWalkers.Update(Building.Efficiency);
        }

        protected bool workerLeaving(WorkerWalker walker)
        {
            var workerPath = Dependencies.Get<IWorkplaceFinder>().GetWorkerPath(Building, null, walker.Worker, walker.Storage, walker.MaxDistance, walker.PathType, walker.PathTag);
            if (workerPath == null)
                return false;
            walker.StartWorking(workerPath);
            return true;
        }

        #region Saving
        [Serializable]
        public class PooledWorkerProviderData
        {
            public PooledWalkerSpawnerData WorkerWalkers;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new PooledWorkerProviderData()
            {
                WorkerWalkers = WorkerWalkers.SaveData()
            });
        }
        public override void LoadData(string json)
        {
            var data = JsonUtility.FromJson<PooledWorkerProviderData>(json);

            WorkerWalkers.LoadData(data.WorkerWalkers);
        }
        #endregion
    }
}