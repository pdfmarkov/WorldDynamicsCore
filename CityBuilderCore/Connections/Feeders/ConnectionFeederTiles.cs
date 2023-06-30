using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// feeds and passes a connection on every point that a tile has on a tilemap, only evaluated at the start
    /// </summary>
    public class ConnectionFeederTiles : ConnectionPasserTiles, IConnectionFeeder
    {
        [Tooltip("value at the point of the feeder")]
        public int Value;
        [Tooltip("how far the value of the feeder carries without falling off")]
        public int Range;
        [Tooltip("value subtracted for every step outside the range")]
        public int Falloff;

        int IConnectionFeeder.Value => Value;
        int IConnectionFeeder.Range => Range;
        int IConnectionFeeder.Falloff => Falloff;

#pragma warning disable 0067
        public event Action<IConnectionFeeder> FeederValueChanged;
#pragma warning restore 0067
    }
}
