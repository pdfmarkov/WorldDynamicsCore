﻿using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// item quantity in global storage
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(ItemScore))]
    public class ItemScore : Score
    {
        /// <summary>
        /// determines how the score is calculated
        /// </summary>
        public enum CalculationMode
        {
            /// <summary>
            /// gets quantity from <see cref="IGlobalStorage"/>
            /// </summary>
            Global = 0,
            /// <summary>
            /// sums up quantities in all <see cref="IStorageComponent"/>s
            /// </summary>
            Stored = 10,
            /// <summary>
            /// sums up quantities in all <see cref="IItemOwner"/>s
            /// </summary>
            Owned = 20,
            /// <summary>
            /// sums up quantities in <see cref="IItemOwner"/> buildings
            /// </summary>
            OwnedBuildings = 21,
            /// <summary>
            /// sums up quantities in <see cref="IItemOwner"/> walkers
            /// </summary>
            OwnedWalkers = 22
        }

        [Tooltip("score is quantity of this item in global storage")]
        public Item Item;
        [Tooltip(@"determines how item quantity is calculated
Global		global storage
Stored		storage components
Owned		ALL item owners
OwnedBuild	building item owners
OwnedWalker	walker item owners")]
        public CalculationMode Mode;

        public override int Calculate()
        {
            switch (Mode)
            {
                case CalculationMode.Global:
                    return Item.GetGlobalQuantity();
                case CalculationMode.Stored:
                    return Item.GetStoredQuantity();
                case CalculationMode.Owned:
                    return Item.GetBuildingOwnedQuantity() + Item.GetWalkerOwnedQuantity();
                case CalculationMode.OwnedBuildings:
                    return Item.GetBuildingOwnedQuantity();
                case CalculationMode.OwnedWalkers:
                    return Item.GetWalkerOwnedQuantity();
                default:
                    return Item.GetGlobalQuantity();
            }
        }
    }
}