using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// bundels of services for whenever instead of a specific service just a general type of service is needed<br/>
    /// eg housing needs two types of religions to evolve from whatever god is available
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(ServiceCategory))]
    public class ServiceCategory : KeyedObject, IBuildingValue, IWalkerValue
    {
        [Tooltip("name when refering to one risk(god)")]
        public string NameSingular;
        [Tooltip("name when refering to multiple risks(2 gods)")]
        public string NamePlural;
        [Tooltip("then services that are part of this category")]
        public Service[] Services;

        public string GetName(int quantity)
        {
            if (quantity > 1)
                return $"{quantity} {NamePlural}";
            else
                return NameSingular;
        }

        public bool HasValue(IBuilding building) => building.GetBuildingComponents<IBuildingComponent>().OfType<IServiceRecipient>().Any(r => Services.Any(s => r.HasServiceValue(s)));
        public float GetMaximum(IBuilding building) => Services.Length * 100f;
        public float GetValue(IBuilding building) => building.GetBuildingComponents<IBuildingComponent>().OfType<IServiceRecipient>().Sum(r => Services.Where(s => r.HasServiceValue(s)).Sum(s => r.GetServiceValue(s)));
        public Vector3 GetPosition(IBuilding building) => building.WorldCenter;

        public bool HasValue(Walker walker) => walker is ServiceWalker serviceWalker && Services.Contains(serviceWalker.Service);
        public float GetMaximum(Walker _) => 1f;
        public float GetValue(Walker walker) => HasValue(walker) ? 1f : 0f;
        public Vector3 GetPosition(Walker walker) => walker.Pivot.position;
    }
}