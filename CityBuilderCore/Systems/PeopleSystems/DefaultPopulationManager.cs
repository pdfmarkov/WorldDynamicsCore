using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// default implementation of all people systems bundeld up into one for convenience<br/>
    /// </summary>
    public class DefaultPopulationManager : MonoBehaviour, IPopulationManager, IEmploymentManager, IWorkplaceFinder
    {
        private Migration[] _migrations;
        private Dictionary<Population, EmploymentPopulation> _populations = new Dictionary<Population, EmploymentPopulation>();
        private Dictionary<string, int> _employmentGroupPriorities = new Dictionary<string, int>();

        protected virtual void Awake()
        {
            _migrations = FindObjectsOfType<Migration>();

            Dependencies.Register<IPopulationManager>(this);
            Dependencies.Register<IEmploymentManager>(this);
            Dependencies.Register<IWorkplaceFinder>(this);
        }

        protected virtual void Start()
        {
            foreach (var population in Dependencies.Get<IObjectSet<Population>>().Objects)
            {
                if (_populations.ContainsKey(population))
                    continue;
                _populations.Add(population, new EmploymentPopulation(population));
            }

            foreach (var employmentGroup in Dependencies.Get<IObjectSet<EmploymentGroup>>().Objects)
            {
                if (_employmentGroupPriorities.ContainsKey(employmentGroup.Key))
                    continue;
                _employmentGroupPriorities.Add(employmentGroup.Key, employmentGroup.Priority);
            }

            this.StartChecker(CheckEmployment);
        }

        #region Population
        public Migration GetMigration(Population population) => _migrations.FirstOrDefault(m => m.Population == population);

        public IEnumerable<IHousing> GetHousings()
        {
            return Dependencies.Get<IBuildingManager>().GetBuildingTraits<IHousing>();
        }
        public int GetQuantity(Population population, bool includeReserved = false)
        {
            return GetHousings().Select(h => h.GetQuantity(population, includeReserved)).DefaultIfEmpty().Sum();
        }
        public int GetCapacity(Population population)
        {
            return GetHousings().Sum(h => h.GetCapacity(population));
        }
        public int GetRemainingCapacity(Population population)
        {
            return GetHousings().Select(h => h.GetRemainingCapacity(population)).DefaultIfEmpty().Sum();
        }

        public void AddHomeless(Population population, IHousing housing, int quantity)
        {
            var migration = _migrations?.FirstOrDefault(m => m.Population == population);
            if (migration == null)
                return;
            migration.AddHomeless(housing, quantity);
        }
        #endregion

        #region Employment
        public void AddEmployment(IEmployment employment)
        {
            foreach (var population in employment.GetPopulations())
            {
                if (!_populations.ContainsKey(population))
                    _populations.Add(population, new EmploymentPopulation(population));
                _populations[population].Add(employment);
            }
        }
        public void RemoveEmployment(IEmployment employment)
        {
            foreach (var population in employment.GetPopulations())
            {
                if (!_populations.ContainsKey(population))
                    continue;
                _populations[population].Remove(employment);
                if (_populations[population].IsEmpty)
                    _populations.Remove(population);
            }
        }

        public int GetNeeded(Population population, EmploymentGroup group = null)
        {
            if (!_populations.ContainsKey(population))
                return 0;

            var employmentPopulation = _populations[population];
            if (group == null)
                return employmentPopulation.WorkersNeeded;

            return employmentPopulation.GetNeeded(group);
        }
        public int GetAvailable(Population population, EmploymentGroup group = null)
        {
            if (!_populations.ContainsKey(population))
                return 0;

            var employmentPopulation = _populations[population];
            if (group == null)
                return employmentPopulation.WorkersAvailable;

            return employmentPopulation.GetAvailable(group);
        }
        public int GetEmployed(Population population, EmploymentGroup group = null) => Mathf.Min(GetNeeded(population, group), GetAvailable(population, group));

        public float GetEmploymentRate(Population population)
        {
            if (_populations.ContainsKey(population))
                return _populations[population].EmploymentRate;
            else
                return 0f;
        }
        public float GetWorkerRate(Population population)
        {
            if (_populations.ContainsKey(population))
                return _populations[population].WorkerRate;
            else
                return 0f;
        }

        public void CheckEmployment()
        {
            foreach (var population in _populations.Keys)
            {
                _populations[population].CalculateNeeded();
                _populations[population].Distribute(GetQuantity(population), _employmentGroupPriorities);
            }
        }

        public int GetPriority(EmploymentGroup group)
        {
            return _employmentGroupPriorities[group.Key];
        }
        public void SetPriority(EmploymentGroup group, int priority)
        {
            _employmentGroupPriorities[group.Key] = priority;
        }
        #endregion

        #region Work
        public WorkerPath GetWorkerPath(IBuilding building, Vector2Int? currentPoint, Worker worker, ItemStorage storage, float maxDistance, PathType pathType, object pathTag)
        {
            if (building == null || worker == null)
                return null;

            List<IWorkerUser> workerUsers = new List<IWorkerUser>();
            foreach (var workerUser in Dependencies.Get<IBuildingManager>().GetBuildingTraits<IWorkerUser>())
            {
                var distance = Vector2.Distance(workerUser.Building.WorldCenter, building.WorldCenter);
                if (distance > maxDistance)
                    continue;

                if (workerUser.GetWorkerNeed(worker) == 0f)
                    continue;

                workerUsers.Add(workerUser);
            }

            if (workerUsers.Count == 1)
            {
                var workerUser = workerUsers[0];

                var path = PathHelper.FindPath(building, currentPoint, workerUser.Building, pathType, pathTag);
                if (path == null)
                    return null;

                WorkerPath workerPath = new WorkerPath(workerUser.Reference, path);

                var items = workerUsers[0].GetItemsNeeded(worker);
                if (items != null)
                {
                    items.Quantity = Math.Min(items.Quantity, storage.GetItemCapacity(items.Item));
                    var supplyPath = Dependencies.Get<IGiverPathfinder>().GetGiverPath(building, currentPoint, items, maxDistance, pathType, pathTag);
                    if (supplyPath == null)
                        return null;

                    workerPath.AddSupply(supplyPath.Component, items, supplyPath.Path);
                }

                return workerPath;
            }
            else if (workerUsers.Count > 1)
            {
                float maxScore = float.MinValue;
                WorkerPath maxScorePath = null;

                foreach (var workerUser in workerUsers)
                {
                    var path = PathHelper.FindPath(building, currentPoint, workerUser.Building, pathType, pathTag);
                    if (path == null)
                        continue;

                    WorkerPath workerPath = new WorkerPath(workerUser.Reference, path);

                    var items = workerUser.GetItemsNeeded(worker);
                    if (items != null)
                    {
                        var supplyPath = Dependencies.Get<IGiverPathfinder>().GetGiverPath(building, currentPoint, items, maxDistance, pathType, pathTag);
                        if (supplyPath == null)
                            continue;

                        workerPath.AddSupply(supplyPath.Component, items, supplyPath.Path);
                    }

                    var score = workerUser.GetWorkerNeed(worker);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxScorePath = workerPath;
                    }
                }

                return maxScorePath;
            }

            return null;
        }
        #endregion

        #region Saving
        [Serializable]
        public class PopulationData
        {
            public Migration.MigrationData[] Migrations;
        }

        string IPopulationManager.SaveData() => JsonUtility.ToJson(new PopulationData()
        {
            Migrations = _migrations?.Select(m => m.SaveData()).ToArray()
        });
        void IPopulationManager.LoadData(string json)
        {
            var data = JsonUtility.FromJson<PopulationData>(json);

            if (_migrations == null)
                return;

            foreach (var migration in _migrations)
            {
                migration.LoadData(data.Migrations.First(m => m.Population == migration.Population.Key));
            }
        }

        [Serializable]
        public class EmploymentData
        {
            public EmploymentGroupData[] Groups;
        }
        [Serializable]
        public class EmploymentGroupData
        {
            public string Key;
            public int Priority;
        }

        string IEmploymentManager.SaveData() => JsonUtility.ToJson(new EmploymentData()
        {
            Groups = _employmentGroupPriorities.Select(p => new EmploymentGroupData() { Key = p.Key, Priority = p.Value }).ToArray()
        });
        void IEmploymentManager.LoadData(string json)
        {
            var data = JsonUtility.FromJson<EmploymentData>(json);

            foreach (var group in data.Groups)
            {
                _employmentGroupPriorities[group.Key] = group.Priority;
            }
        }
        #endregion
    }
}