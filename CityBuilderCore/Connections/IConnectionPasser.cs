using System;
using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// any object that lets a connection pass through its points<br/>
    /// gets notified when any of the values at those points change
    /// </summary>
    public interface IConnectionPasser
    {
        /// <summary>
        /// the connection this passer allows to pass
        /// </summary>
        Connection Connection { get; }

        /// <summary>
        /// fired by the passer when points are removed or added to it
        /// </summary>
        event Action<PointsChanged<IConnectionPasser>> PointsChanged;

        /// <summary>
        /// the points of the passer
        /// </summary>
        /// <returns></returns>
        IEnumerable<Vector2Int> GetPoints();
        /// <summary>
        /// this is where the passer gets notified about changes to the values of its points
        /// </summary>
        /// <param name="point">the point at which the value changed</param>
        /// <param name="value">the new value at the point</param>
        void ValueChanged(Vector2Int point, int value);
    }
}
