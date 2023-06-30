namespace CityBuilderCore.Tests
{
    public class DebugRoamer : RoamingWalker
    {
        protected override void Start()
        {
            base.Start();

            delay(() => Initialize(null, Dependencies.Get<IGridPositions>().GetGridPosition(transform.position)));
        }
    }
}