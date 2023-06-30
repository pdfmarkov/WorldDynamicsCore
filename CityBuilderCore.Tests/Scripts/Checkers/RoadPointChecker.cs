using NUnit.Framework;
using System.Linq;

namespace CityBuilderCore.Tests
{
    public class RoadPointChecker : CheckerBase
    {
        public Road ExpectedRoad;

        public override void Check()
        {
            var point = Dependencies.Get<IGridPositions>().GetGridPosition(transform.position);

            var roadNetwork = Dependencies.Get<IStructureManager>().GetStructures(point).FirstOrDefault() as RoadNetwork;

            Road actualRoad = null;

            if (roadNetwork != null)
            {
                roadNetwork.TryGetRoad(point, out actualRoad, out string _);
            }

            Assert.AreEqual(ExpectedRoad, actualRoad, name);
        }
    }
}
