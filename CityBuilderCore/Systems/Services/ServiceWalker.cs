using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// roams around and fills the services of <see cref="IServiceRecipient"/> while it is in range
    /// </summary>
    public class ServiceWalker : BuildingComponentWalker<IServiceRecipient>
    {
        [Tooltip("the service that this walker will refill if it passes a fitting service recipient")]
        public Service Service;
        [Tooltip("service increase per second(100 refills a service completely in 1 second)")]
        public float Amount = 100f;

        protected override void onComponentRemaining(IServiceRecipient buildingComponent)
        {
            base.onComponentRemaining(buildingComponent);

            buildingComponent.ModifyService(Service, Amount * Time.deltaTime);
        }
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class ManualServiceWalkerSpawner : ManualWalkerSpawner<ServiceWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class CyclicServiceWalkerSpawner : CyclicWalkerSpawner<ServiceWalker> { }
    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class PooledServiceWalkerSpawner : PooledWalkerSpawner<ServiceWalker> { }
}