using System;

namespace CityBuilderCore
{
    /// <summary>
    /// helper class for <see cref="Walker"/> that hold the current status when the walker is following a <see cref="WalkingPath"/><br/>
    /// </summary>
    public class WalkingState
    {
        /// <summary>
        /// the walking path the walker is currently following
        /// </summary>
        public WalkingPath WalkingPath { get; private set; }
        /// <summary>
        /// how far the walker has moved within the current step, used to calculate the actual position of the walker by interpolating between the last and next point
        /// </summary>
        public float Moved { get; set; }
        /// <summary>
        /// index of the point in the path the walker has last visited
        /// </summary>
        public int Index { get; set; }

        private WalkingState()
        {

        }
        public WalkingState(WalkingPath walkingPath)
        {
            WalkingPath = walkingPath;
        }

        public void Recalculated(WalkingPath walkingPath)
        {
            WalkingPath = walkingPath;
            Index = 0;
        }

        public void Cancel()
        {
            WalkingPath.Cancel();
        }

        #region Saving
        [Serializable]
        public class WalkingData
        {
            public WalkingPath.WalkingPathData WalkingPathData;
            public float Delay;
            public float Moved;
            public int Index;
        }

        public WalkingData GetData() => new WalkingData()
        {
            WalkingPathData = WalkingPath.GetData(),
            Moved = Moved,
            Index = Index
        };
        public static WalkingState FromData(WalkingData data)
        {
            if (data == null)
                return null;
            return new WalkingState()
            {
                WalkingPath = WalkingPath.FromData(data.WalkingPathData),
                Moved = data.Moved,
                Index = data.Index
            };
        }
        #endregion
    }
}