using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// moves population quantities into and out of the map depending on the current sentiment
    /// </summary>
    public class Migration : MonoBehaviour
    {
        [Tooltip("walkers that move population into housing, leave prefab empty to move population in without walker")]
        public ManualImmigrationWalkerSpawner ImmigrationWalkers;
        [Tooltip("walkers that move population out of housing, leave prefab empty to move population out without walker")]
        public ManualEmigrationWalkerSpawner EmigrationWalkers;
        [Tooltip("walkers for population that got thrown out, leave prefab empty to not spawn")]
        public ManualHomelessWalkerSpawner HomelessWalkers;

        [Tooltip("start point for immigration walkers and end point for emigration walkers")]
        public Transform Entry;
        [Tooltip("type of population migrating, add one migration per population")]
        public Population Population;
        [Tooltip("check interval for immigration and emigration, combined with sentiment controls the rate at which people can move into or out of your city")]
        public float Interval = 1f;
        [Tooltip("positive sentiment makes people move to your city while negative sentiment makes them leave, higher numbers increase the rate(sentiment*interval)")]
        public float Sentiment = 1f;
        [Tooltip("optional, set to pull sentiment from a score(-100 to 100) instead of using the manual sentiment value above")]
        public Score SentimentScore;
        [Tooltip("Minumum amount of people that always immigrates before the sentiment value counts")]
        public int Minimum = 0;

        public Vector2Int EntryPosition => Dependencies.Get<IGridPositions>().GetGridPosition(Entry.position);

        private IPopulationManager _populationManager;

        private void Awake()
        {
            var assembly = typeof(Walker).Assembly;
            var check = assembly.GetTypes().SelectMany(t => t.GetFields()).Where(f => f.FieldType.Assembly == assembly && f.FieldType.IsGenericType).Select(f => f.FieldType + "-" + f.Name).ToList();

            ImmigrationWalkers.Initialize(transform);
            EmigrationWalkers.Initialize(transform);
            HomelessWalkers.Initialize(transform);
        }

        private void Start()
        {
            _populationManager = Dependencies.Get<IPopulationManager>();

            StartCoroutine(checkMigration());
        }

        private IEnumerator checkMigration()
        {
            while (true)
            {
                yield return new WaitForSeconds(Interval / Sentiment == 0f ? 1f : Mathf.Abs(Sentiment));

                if (HomelessWalkers.CurrentWalkers.Where(h => !h.IsAssigned).Any())//check for homeless housing
                {
                    if (_populationManager.GetRemainingCapacity(Population) > 0)
                    {
                        foreach (var homeless in HomelessWalkers.CurrentWalkers.Where(h => !h.IsAssigned).ToList())
                        {
                            var housing = _populationManager.GetHousings().OrderByDescending(h => h.GetRemainingCapacity(Population)).FirstOrDefault();
                            if (housing == null)
                                break;

                            homeless.AssignHousing(housing.Reference);
                        }
                    }
                }

                var sentiment = Sentiment;
                if (SentimentScore)
                    sentiment = Dependencies.Get<IScoresCalculator>().GetValue(SentimentScore) / 100f;

                if (sentiment > 0f || (Minimum > 0 && _populationManager.GetQuantity(Population, true) < Minimum))//immigrate
                {
                    if (_populationManager.GetRemainingCapacity(Population) <= 0)
                        continue;

                    var housing = _populationManager.GetHousings().OrderByDescending(h => h.GetRemainingCapacity(Population)).FirstOrDefault();
                    if (housing == null)
                        continue;

                    if (ImmigrationWalkers.Prefab)
                    {
                        var path = PathHelper.FindPath(EntryPosition, housing.Building, ImmigrationWalkers.Prefab.PathType, ImmigrationWalkers.Prefab.PathTag);
                        if (path == null)
                            continue;

                        if (!ImmigrationWalkers.HasWalker)
                            continue;

                        ImmigrationWalkers.Spawn(w => w.StartImmigrating(housing.Reference, path, Population), EntryPosition);
                    }
                    else
                    {
                        housing.Inhabit(Population, housing.GetRemainingCapacity(Population));
                    }
                }
                else if (Sentiment < 0f)//emigrate
                {
                    int quantity = _populationManager.GetQuantity(Population);
                    if (quantity <= 0)
                        continue;

                    var housing = _populationManager.GetHousings().OrderByDescending(h => h.GetQuantity(Population)).FirstOrDefault();

                    if (housing == null)
                        continue;

                    if (EmigrationWalkers.Prefab)
                    {
                        if (!EmigrationWalkers.HasWalker)
                            continue;

                        housing.Abandon(Population, EmigrationWalkers.Prefab.Capacity);

                        var path = PathHelper.FindPath(housing.Building, EntryPosition, EmigrationWalkers.Prefab.PathType, EmigrationWalkers.Prefab.PathTag);
                        if (path != null)
                            EmigrationWalkers.Spawn(w => w.StartEmigrating(path), EntryPosition);
                    }
                    else
                    {
                        housing.Abandon(Population, 1);
                    }
                }
            }
        }

        public void AddHomeless(IHousing housing, int quantity)
        {
            if (!HomelessWalkers.Prefab)
                return;

            var accessPoint = housing.Building.GetAccessPoint(HomelessWalkers.Prefab.PathType, HomelessWalkers.Prefab.PathTag);
            if (!accessPoint.HasValue)
                return;

            while (quantity > 0)
            {
                int walkerQuantity = Mathf.Min(HomelessWalkers.Prefab.Capacity, quantity);
                HomelessWalkers.Spawn(w => w.StartHomelessing(walkerQuantity, Population), accessPoint.Value);
                quantity -= walkerQuantity;
            }
        }

        #region Saving
        [Serializable]
        public class MigrationData
        {
            public string Population;
            public ManualWalkerSpawnerData ImmigrationWalkers;
            public ManualWalkerSpawnerData EmigrationWalkers;
            public ManualWalkerSpawnerData HomelessWalkers;
        }

        public MigrationData SaveData() => new MigrationData()
        {
            Population = Population.Key,
            ImmigrationWalkers = ImmigrationWalkers.SaveData(),
            EmigrationWalkers = EmigrationWalkers.SaveData(),
            HomelessWalkers = HomelessWalkers.SaveData()
        };
        public void LoadData(MigrationData data)
        {
            ImmigrationWalkers.LoadData(data.ImmigrationWalkers);
            EmigrationWalkers.LoadData(data.EmigrationWalkers);
            HomelessWalkers.LoadData(data.HomelessWalkers);
        }
        #endregion
    }
}