using CityBuilderCore;
using System;

namespace CityBuilderUrban
{
    public class TruckWalker : BuildingComponentWalker<ShopComponent>
    {
        public ItemQuantity Items;

        protected override void onComponentEntered(ShopComponent shop)
        {
            base.onComponentEntered(shop);

            shop.Supply(Items);
        }
    }

    /// <summary>
    /// concrete implementation for serialization, not needed starting unity 2020.1
    /// </summary>
    [Serializable]
    public class ManualTruckWalkerSpawner : ManualWalkerSpawner<TruckWalker> { }
}
