using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CityBuilderCore
{
    public class Wanderer : MonoBehaviour, ISaveData
    {
        public enum WandererState
        {
            Waiting = 0,
            Wandering = 10
        }

        public TileBase[] Tiles;
        public string Tilemap;
        public Transform Pivot;
        public float Interval;
        public float Speed;
        public float TimePerStep => 1 / Speed;
        public string Key;

        public IntEvent StateChanged;

        private Tilemap _tilemap;
        private WandererState _state = WandererState.Waiting;
        private float _time;
        private Vector3 _start;
        private Vector3 _target;
        private float _scale;

        private void Start()
        {
            Pivot.position += Dependencies.Get<IMap>().GetVariance();

            if (Tiles != null && Tiles.Length > 0)
                _tilemap = FindObjectsOfType<Tilemap>().FirstOrDefault(t => t.name == Tilemap);
        }

        private void Update()
        {
            switch (_state)
            {
                case WandererState.Waiting:
                    _time += Time.deltaTime;
                    if (_time >= Interval)
                    {
                        wander();
                        StateChanged?.Invoke((int)_state);
                    }
                    break;
                case WandererState.Wandering:
                    _time += Time.deltaTime * _scale;
                    if (_time >= TimePerStep)
                    {
                        transform.position = _target;

                        _state = WandererState.Waiting;
                        StateChanged?.Invoke((int)_state);
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(_start, _target, _time / TimePerStep);
                    }
                    break;
                default:
                    break;
            }
        }

        private void wander()
        {
            var candidates = new List<Vector3>();
            var positions = Dependencies.Get<IGridPositions>();

            foreach (var position in PositionHelper.GetAdjacent(positions.GetGridPosition(transform.position), Vector2Int.one, true))
            {
                if (_tilemap == null || Tiles.Contains(_tilemap.GetTile((Vector3Int)position)))
                    candidates.Add(positions.GetWorldPosition(position));
            }

            if (candidates.Count == 0)
            {
                _state = WandererState.Waiting;
                _time = 0f;
            }
            else
            {
                _state = WandererState.Wandering;
                _time = 0f;
                _start = transform.position;
                _target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                _scale = 1 / Vector3.Distance(_start, _target);

                Dependencies.Get<IGridRotations>().SetRotation(Pivot, _target - _start);
            }
        }

        #region Saving
        [Serializable]
        public class WandererData
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public int State;
            public float Time;
            public Vector3 Start;
            public Vector3 Target;
            public float Scale;
        }

        public string SaveData()
        {
            return JsonUtility.ToJson(new WandererData()
            {
                Position = transform.position,
                Rotation = Pivot.localRotation,

                State = (int)_state,
                Time = _time,
                Start = _start,
                Target = _target,
                Scale = _scale
            });
        }

        public void LoadData(string json)
        {
            var data = JsonUtility.FromJson<WandererData>(json);

            transform.position = data.Position;
            Pivot.localRotation = data.Rotation;

            _state = (WandererState)data.State;
            _time = data.Time;
            _start = data.Start;
            _target = data.Target;
            _scale = data.Scale;
        }
        #endregion
    }
}