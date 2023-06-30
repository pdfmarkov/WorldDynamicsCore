using CityBuilderCore;
using System;
using System.Linq;
using UnityEngine;

namespace CityBuilderUrban
{
    public class UrbanManager : ExtraDataBehaviour
    {
        [Serializable]
        public class RunningCost
        {
            public BuildingInfo Info;
            public ItemQuantity Items;
        }

        public ManualTornadoWalkerSpawner Tornados;

        public RunningCost[] Costs;

        public Transform[] Entries;
        public Transform[] Exits;

        public MoneyVisual MoneyVisual;

        private void Awake()
        {
            Dependencies.Register(this);

            Tornados.Initialize(transform);
        }

        public void RemoveRunningCost()
        {
            var buildingManager = Dependencies.Get<IBuildingManager>();
            var globalStorage = Dependencies.Get<IGlobalStorage>();

            foreach (var cost in Costs)
            {
                foreach (var building in buildingManager.GetBuildings(cost.Info))
                {
                    globalStorage.Items.RemoveItems(cost.Items.Item, cost.Items.Quantity);

                    VisualizeMoney(building.Root.position, -cost.Items.Quantity);
                }
            }
        }

        public void VisualizeMoney(Vector3 position, int quantity)
        {
            if (quantity == 0)
                return;

            var instance = Instantiate(MoneyVisual, position, Quaternion.identity);
            instance.Set(quantity);
        }

        public Vector2Int GetRailwayEntry() => Dependencies.Get<IGridPositions>().GetGridPosition(Entries.Random().position);
        public Vector2Int GetRailwayExit() => Dependencies.Get<IGridPositions>().GetGridPosition(Exits.Random().position);

        public BuildingComponentPath<ShopComponent> GetShopPath(BuildingReference home, PathType pathType, object pathTag = null)
        {
            if (pathTag is null)
            {
                throw new ArgumentNullException(nameof(pathTag));
            }

            foreach (var shop in Dependencies.Get<IBuildingManager>()
                                              .GetBuildings()
                                              .Select(b => b.GetBuildingComponent<ShopComponent>())
                                              .Where(s => s != null)
                                              .OrderBy(s => Vector3.Distance(s.Building.WorldCenter, home.Instance.WorldCenter)))
            {
                var path = PathHelper.FindPath(home.Instance, shop.Building, pathType, pathTag);
                if (path == null)
                    continue;

                return new BuildingComponentPath<ShopComponent>(shop.Reference, path);
            }

            return null;
        }

        public void SpawnTornado() => Tornados.Spawn(start: Vector2Int.zero);

        public override void LoadData(string json) => Tornados.LoadData(JsonUtility.FromJson<ManualWalkerSpawnerData>(json));
        public override string SaveData() => JsonUtility.ToJson(Tornados.SaveData());
    }
}
