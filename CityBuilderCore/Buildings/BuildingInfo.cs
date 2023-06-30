using System;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// meta info for data that does not change between instances of a building<br/>
    /// can be used to compare buildings(is that building a silo?)
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(BuildingInfo))]
    public class BuildingInfo : KeyedObject
    {
        [Tooltip("display name")]
        public string Name;
        [Tooltip("display description")]
        [TextArea]
        public string Description;
        [Tooltip("size of the building on the grid")]
        public Vector2Int Size;
        [Tooltip("can the building be removed by regular means")]
        public bool IsDestructible = true;
        [Tooltip("whether grid walkers can traverse the points of this building")]
        public bool IsWalkable = false;
        [Tooltip("prefab instantiated when the building is built")]
        public Building Prefab;
        [Tooltip("alternative prefabs for the same building that can be used to give some visual variety")]
        public Building[] PrefabAlternatives;
        [Tooltip("prefab used to display building during building")]
        public GameObject Ghost;
        [Tooltip("alternative ghosts that can be used for PrefabAlternatives")]
        public GameObject[] GhostAlternatives;
        [Tooltip("use any point to access building or a special AccessPoint")]
        public BuildingAccessType AccessType;
        [Tooltip("point used for access when AccessType is Preferred or Exclusive")]
        public Vector2Int AccessPoint;
        [Tooltip("Items to be subtracted from GlobalStorage for building")]
        public ItemQuantity[] Cost;
        [Tooltip(@"map based requirements for building can be either layer or map-ground based or both, just leave the one you dont need empty
by default all the requirements have to be met")]
        public BuildingRequirement[] BuildingRequirements;
        [Tooltip("road based requirements for building, specify which points of the building need what kind of road and whether the road is automatically added")]
        public RoadRequirement[] RoadRequirements;
        [Tooltip("determines which structures can reside in the same position")]
        public StructureLevelMask Level;

        public Building GetPrefab(int index)
        {
            if (index == 0 || PrefabAlternatives == null || PrefabAlternatives.Length == 0)
                return Prefab;

            index = index % (PrefabAlternatives.Length + 1);

            if (index == 0)
                return Prefab;
            else
                return PrefabAlternatives[index - 1];
        }
        public int GetPrefabIndex(MonoBehaviour prefab)
        {
            if (prefab == Prefab)
                return 0;
            return Array.IndexOf(PrefabAlternatives, prefab) + 1;
        }
        public GameObject GetGhost(int index)
        {
            if (index == 0 || GhostAlternatives == null || GhostAlternatives.Length == 0)
                return Ghost;

            index = index % (GhostAlternatives.Length + 1);

            if (index == 0)
                return Ghost;
            else
                return GhostAlternatives[index - 1];
        }

        public virtual bool CheckBuildingRequirements(Vector2Int point, BuildingRotation rotation)
        {
            if (BuildingRequirements != null && BuildingRequirements.Any(r => !r.IsFulfilled(point, Size, rotation)))
                return false;
            if (RoadRequirements != null && RoadRequirements.Any(r => !Dependencies.Get<IRoadManager>().CheckRequirement(rotation.RotateBuildingPoint(point, r.Point, Size), r)))
                return false;
            return true;
        }
        public virtual bool CheckBuildingAvailability(Vector2Int point)
        {
            return Dependencies.Get<IStructureManager>().CheckAvailability(point, Level.Value);
        }

        public virtual void Prepare(Vector2Int point, BuildingRotation rotation)
        {
            if (RoadRequirements != null)
                RoadRequirements.Where(r => r.Amend && r.Road).ForEach(r => Dependencies.Get<IRoadManager>().Add(new[] { rotation.RotateBuildingPoint(point, r.Point, Size) }, r.Road));
        }

        public virtual IBuilding Create(DefaultBuildingManager.BuildingMetaData metaData, Transform parent)
        {
            var rotation = BuildingRotation.Create(metaData.Rotation);
            var building = Instantiate(GetPrefab(metaData.Index), Dependencies.Get<IGridPositions>().GetWorldPosition(rotation.RotateOrigin(metaData.Position, Size)), rotation.GetRotation(), parent);

            building.Initialize();
            building.Id = new Guid(metaData.Id);

            return building;
        }
    }
}