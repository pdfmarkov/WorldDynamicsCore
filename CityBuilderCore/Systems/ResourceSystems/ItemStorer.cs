using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// Utility class that adds items to an <see cref="IItemOwner"/> on the same gameobject when is starts, mostly used for testing
    /// </summary>
    public class ItemStorer : MonoBehaviour
    {
        [Tooltip("items that will be added to the item owner on the same gameobject")]
        public ItemQuantity[] ItemQuantities;

        private void Start()
        {
            foreach (var itemQuantity in ItemQuantities)
            {
                GetComponent<IItemOwner>().ItemContainer.AddItems(itemQuantity.Item, itemQuantity.Quantity);
            }
        }
    }
}
