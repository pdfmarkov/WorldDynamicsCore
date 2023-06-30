using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// base class for spawning and keeping track of walkers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WalkerSpawner<T>
    where T : Walker
    {
        [Tooltip("the walker prefab that will be instantiated when spawning")]
        public T Prefab;
        [Tooltip("maximum active walkers")]
        public int Count = 1;

        /// <summary>
        /// whether another walker can be spawned
        /// </summary>
        public bool HasWalker => Prefab && (Count == -1 || _currentWalkers.Count < Count);

        public IReadOnlyList<T> CurrentWalkers => _currentWalkers;

        protected List<T> _currentWalkers = new List<T>();

        private Transform _root;
        private IBuilding _building;

        private Func<T, bool> _onSpawning;
        private Action<T> _onFinished;

        public void Initialize(Transform root, Func<T, bool> onSpawning = null, Action<T> onFinished = null)
        {
            _root = root;
            _onSpawning = onSpawning;
            _onFinished = onFinished;

            initialize();
        }
        public void Initialize(IBuilding building, Func<T, bool> onSpawning = null, Action<T> onFinished = null)
        {
            _building = building;
            _onSpawning = onSpawning;
            _onFinished = onFinished;

            initialize();
        }
        protected virtual void initialize()
        {

        }

        public void Integrate(T walker, Action<T> onSpawned = null)
        {
            walker.Initialize(_building?.BuildingReference, walker.GridPoint);
            walker.Spawned();

            _currentWalkers.Add(walker);

            walker.Finished += walkerFinished;

            onSpawned?.Invoke(walker);
        }

        protected void clearWalkers()
        {
            var pool = Dependencies.GetOptional<IObjectPool>();
            var manager = Dependencies.Get<IWalkerManager>();

            _currentWalkers.ForEach(w => manager.DeregisterWalker(w));

            if (pool == null)
                _currentWalkers.ForEach(w => UnityEngine.Object.Destroy(w.gameObject));
            else
                _currentWalkers.ForEach(w => pool.Release(Prefab, w));

            _currentWalkers.Clear();
        }

        protected T reloadActive()
        {
            T walker = UnityEngine.Object.Instantiate(Prefab, _root ? _root : _building.Root);

            Dependencies.Get<IWalkerManager>().RegisterWalker(walker);

            _currentWalkers.Add(walker);

            walker.Finished += walkerFinished;

            return walker;
        }

        protected T spawn(Action<T> onSpawned = null, Vector2Int? start = null)
        {
            if (!start.HasValue)
                start = _building.GetAccessPoint(Prefab.PathType, Prefab.PathTag);

            if (!start.HasValue)
                return null;

            T walker;

            var pool = Dependencies.GetOptional<IObjectPool>();

            if (pool == null)
            {
                walker = UnityEngine.Object.Instantiate(Prefab, _root ? _root : _building.Root, true);
                walker.Initialize(_building?.BuildingReference, start.Value);

                if (_onSpawning != null && !_onSpawning(walker))
                {
                    UnityEngine.Object.Destroy(walker.gameObject);
                    return null;
                }

                walker.Spawned();
            }
            else
            {
                walker = pool.Request(Prefab, _root ? _root : _building.Root, w =>
                {
                    w.transform.rotation = Prefab.transform.rotation;
                    w.Initialize(_building?.BuildingReference, start.Value);

                    if (_onSpawning != null && !_onSpawning(w))
                    {
                        return false;
                    }

                    w.Spawned();

                    return true;
                });

                if (walker == null)
                {
                    return null;
                }
            }

            _currentWalkers.Add(walker);

            walker.Finished += walkerFinished;

            onSpawned?.Invoke(walker);

            return walker;
        }

        private void walkerFinished(Walker walker)
        {
            walker.Finished -= walkerFinished;

            _currentWalkers.Remove((T)walker);
            _onFinished?.Invoke((T)walker);

            var pool = Dependencies.GetOptional<IObjectPool>();

            if (pool == null)
                UnityEngine.Object.Destroy(walker.gameObject);
            else
                pool.Release(Prefab, walker);
        }
    }
}