using NUnit.Framework;

namespace CityBuilderCore.Tests
{
    public class BuildingRequirementChecker : CheckerBase
    {
        public BuildingInfo BuildingInfo;
        public bool Expected;

        public override void Check()
        {
            Assert.AreEqual(Expected, BuildingInfo.CheckBuildingRequirements(Dependencies.Get<IGridPositions>().GetGridPosition(transform.position), BuildingRotation.Create()), name);
        }
    }
}