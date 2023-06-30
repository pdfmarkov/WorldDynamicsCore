using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// helper class for <see cref="Walker"/> that hold the current status when the walker is following a <see cref="WalkingPath"/><br/>
    /// </summary>
    public class WalkingAgentState
    {
        /// <summary>
        /// the walking path the walker is currently following
        /// </summary>
        public WalkingPath WalkingPath { get; private set; }
        public Vector3 Destination { get; private set; }
        /// <summary>
        /// current velocity of the agent
        /// </summary>
        public Vector3 Velocity { get; set; }

        private WalkingAgentState()
        {

        }
        public WalkingAgentState(WalkingPath walkingPath, Vector3 destination)
        {
            WalkingPath = walkingPath;
            Destination = destination;
        }

        public void Recalculated(WalkingPath walkingPath)
        {
            WalkingPath = walkingPath;
        }

        public void Cancel()
        {
            WalkingPath.Cancel();
        }

        #region Saving
        [Serializable]
        public class WalkingAgentData
        {
            public WalkingPath.WalkingPathData WalkingPathData;
            public float Delay;
            public Vector3 Destination;
            public Vector3 Velocity;
        }

        public WalkingAgentData GetData() => new WalkingAgentData()
        {
            WalkingPathData = WalkingPath.GetData(),
            Destination = Destination,
            Velocity = Velocity
        };
        public static WalkingAgentState FromData(WalkingAgentData data)
        {
            if (data == null)
                return null;
            return new WalkingAgentState()
            {
                WalkingPath = WalkingPath.FromData(data.WalkingPathData),
                Destination = data.Destination,
                Velocity = data.Velocity
            };
        }
        #endregion
    }
}