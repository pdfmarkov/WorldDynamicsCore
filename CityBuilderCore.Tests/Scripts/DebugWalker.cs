using UnityEngine;

namespace CityBuilderCore.Tests
{
    public class DebugWalker : Walker
    {
        public Transform Target;
        public bool ShouldArrive;

        public bool HasFinished { get; private set; }
        public bool HasArrived => Dependencies.Get<IGridPositions>().GetGridPosition(transform.position) == Dependencies.Get<IGridPositions>().GetGridPosition(Target.position);

        protected override void Start()
        {
            base.Start();

            Initialize(null, Dependencies.Get<IGridPositions>().GetGridPosition(transform.position));
        }

        public override void Initialize(BuildingReference home, Vector2Int start)
        {
            base.Initialize(home, start);

            if (Target)
                this.Delay(1, () => tryWalk(Dependencies.Get<IGridPositions>().GetGridPosition(Target.position), finished: () => HasFinished = true));
        }
    }
}