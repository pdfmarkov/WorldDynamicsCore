﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// component that feeds into a connection at the buildings points with its value scaled to the buildings efficiency<br/>
    /// used on the power station and water pump in the urban demo
    /// </summary>
    public class ConnectionFeederComponent : BuildingComponent, IConnectionFeeder
    {
        public override string Key => "CFC";

        [Tooltip("the connection that will be fed by this component")]
        public Connection Connection;

        [Tooltip("value at the point of the feeder")]
        public int MaxValue;
        [Tooltip("how far the value of the feeder carries without falling off")]
        public int Range;
        [Tooltip("value subtracted for every step outside the range")]
        public int Falloff;

        int IConnectionFeeder.Value => _value;
        int IConnectionFeeder.Range => Range;
        int IConnectionFeeder.Falloff => Falloff;

#pragma warning disable 0067
        public event Action<PointsChanged<IConnectionPasser>> PointsChanged;
#pragma warning restore 0067
        public event Action<IConnectionFeeder> FeederValueChanged;
        public PointValueEvent PointValueChanged;

        Connection IConnectionPasser.Connection => Connection;

        public IEnumerable<Vector2Int> GetPoints() => Building.GetPoints();

        private int _value;

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            Dependencies.Get<IConnectionManager>().Register(this);
        }
        public override void OnReplacing(IBuilding replacement)
        {
            base.OnReplacing(replacement);

            Dependencies.Get<IConnectionManager>().Deregister(this);
            var passerReplacement = replacement.GetBuildingComponents<ConnectionPasserComponent>().FirstOrDefault(c => c.Connection == Connection);
            if (passerReplacement != null)
                Dependencies.Get<IConnectionManager>().Register(passerReplacement);
        }
        public override void TerminateComponent()
        {
            base.TerminateComponent();

            Dependencies.Get<IConnectionManager>().Deregister(this);
        }

        public void ValueChanged(Vector2Int point, int value)
        {
            PointValueChanged?.Invoke(point, value);
        }

        private void Update()
        {
            int value = Mathf.RoundToInt(MaxValue * Building.Efficiency);
            if (_value == value)
                return;
            _value = value;
            FeederValueChanged?.Invoke(this);
        }

    }
}