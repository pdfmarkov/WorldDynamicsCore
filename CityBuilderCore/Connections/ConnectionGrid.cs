using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// helper class used by <see cref="DefaultConnectionManager"/> to encapsulate connection handling for one connection
    /// </summary>
    public class ConnectionGrid : ILayerAffector
    {
        public Connection Connection { get; }

        public Layer Layer => Connection.Layer;
        public string Name => Connection.Name;

        public event Action<ILayerAffector> Changed;

        private Dictionary<Vector2Int, ConnectionPoint> _points = new Dictionary<Vector2Int, ConnectionPoint>();
        private List<IConnectionFeeder> _feeders = new List<IConnectionFeeder>();
        private bool _isDirty = true;

        public ConnectionGrid(Connection connection)
        {
            Connection = connection;

            if (Connection.Layer)
                Dependencies.Get<ILayerManager>().Register(this);
        }

        public void RegisterFeeder(IConnectionFeeder feeder)
        {
            _feeders.Add(feeder);
            feeder.FeederValueChanged += feederValueChanged;
            RegisterPasser(feeder);
        }
        public void DeregisterFeeder(IConnectionFeeder feeder)
        {
            _feeders.Remove(feeder);
            feeder.FeederValueChanged -= feederValueChanged;
            DeregisterPasser(feeder);
        }

        public void RegisterPasser(IConnectionPasser passer)
        {
            foreach (var point in passer.GetPoints())
            {
                _points.Add(point, new ConnectionPoint(passer));
            }

            passer.PointsChanged += passerPointsChanged;

            _isDirty = true;
        }

        public void DeregisterPasser(IConnectionPasser passer)
        {
            foreach (var point in passer.GetPoints())
            {
                _points.Remove(point);
            }

            passer.PointsChanged -= passerPointsChanged;

            _isDirty = true;
        }

        public bool HasPoint(Vector2Int point)
        {
            return _points.ContainsKey(point);
        }
        public int GetValue(Vector2Int point)
        {
            if (_points.ContainsKey(point))
                return _points[point].Value;
            return 0;
        }

        public void Check(bool force = false)
        {
            if (_isDirty || force)
            {
                calculate();
                _isDirty = false;
            }
        }

        public Dictionary<Vector2Int, int> GetValues()
        {
            var values = new Dictionary<Vector2Int, int>();

            foreach (var item in _points)
            {
                var value = item.Value.Value;

                for (int i = 0; i < Connection.LayerRange; i++)
                {
                    foreach (var point in PositionHelper.GetAdjacent(item.Key, Vector2Int.one, true, i))
                    {
                        if (values.ContainsKey(point))
                        {
                            if (values[point] < value)
                                values[point] = value;
                        }
                        else
                        {
                            values.Add(point, value);
                        }
                    }

                    value -= Connection.LayerFalloff;
                    if (value <= 0)
                        break;
                }
            }

            return values;
        }

        private void passerPointsChanged(PointsChanged<IConnectionPasser> change)
        {
            foreach (var point in change.RemovedPoints)
            {
                _points.Remove(point);
            }

            foreach (var point in change.AddedPoints)
            {
                _points.Add(point, new ConnectionPoint(change.Sender));
            }

            _isDirty = true;
        }

        private void feederValueChanged(IConnectionFeeder feeder)
        {
            _isDirty = true;
        }

        private void calculate()
        {
            var values = new Dictionary<Vector2Int, int>();

            foreach (var feeder in _feeders)
            {
                List<Vector2Int> feederPoints = new List<Vector2Int>();
                foreach (var point in feeder.GetPoints())
                {
                    if (values.ContainsKey(point))
                        values[point] = feeder.Value;
                    else
                        values.Add(point, feeder.Value);

                    feederPoints.Add(point);
                }

                foreach (var feederPoint in feederPoints)
                {
                    var steps = 0;
                    var value = feeder.Value;

                    var current = new Queue<Vector2Int>();
                    var next = new Queue<Vector2Int>();
                    var visited = new List<Vector2Int>();

                    next.Enqueue(feederPoint);

                    while (value > 0 && next.Count > 0)
                    {
                        var t = current;
                        current = next;
                        next = t;

                        while (current.Count > 0)
                        {
                            var point = current.Dequeue();
                            visited.Add(point);

                            foreach (var adjacent in PositionHelper.GetAdjacent(point, Vector2Int.one))
                            {
                                if (visited.Contains(adjacent))
                                    continue;

                                if (_points.ContainsKey(adjacent))
                                {
                                    if (values.ContainsKey(adjacent))
                                    {
                                        if (values[adjacent] < value)
                                        {
                                            values[adjacent] = value;
                                        }
                                    }
                                    else
                                    {
                                        values.Add(adjacent, value);
                                    }

                                    if (!next.Contains(adjacent))
                                        next.Enqueue(adjacent);
                                }
                            }
                        }

                        steps++;
                        if (steps >= feeder.Range)
                        {
                            if (feeder.Falloff > 0)
                                value -= feeder.Falloff;
                            else
                                value = 0;
                        }
                    }
                }
            }

            foreach (var point in _points.Keys)
            {
                var value = values.ContainsKey(point) ? values[point] : 0;

                var connectionPoint = _points[point];
                if (value == connectionPoint.Value)
                    continue;
                connectionPoint.Value = value;
                connectionPoint.Passer.ValueChanged(point, value);
            }

            Changed?.Invoke(this);
        }

    }
}
