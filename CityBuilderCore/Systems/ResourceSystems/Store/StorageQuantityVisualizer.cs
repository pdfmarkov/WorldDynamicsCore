using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// generates visuals for the items stored in a stacked <see cref="IStorageComponent"/><br/>
    /// the visuals defined in <see cref="Item"/> are used
    /// </summary>
    [RequireComponent(typeof(IStorageComponent))]
    public class StorageQuantityVisualizer : MonoBehaviour
    {
        private class StorageQuantityItem
        {
            public Transform Origin;
            public StorageQuantityVisual Visual;
        }

        [Tooltip("define one transform for every stack in the storage")]
        public Transform[] Origins;
        [Tooltip("items can have multiple visuals for different storages")]
        public int VisualIndex;

        private IStorageComponent _storageComponent;
        private Dictionary<ItemStack, StorageQuantityItem> _items = new Dictionary<ItemStack, StorageQuantityItem>();

        private void Awake()
        {
            _storageComponent = GetComponent<IStorageComponent>();
        }

        private void Start()
        {
            for (int i = 0; i < _storageComponent.Storage.Stacks.Length; i++)
            {
                var stack = _storageComponent.Storage.Stacks[i];
                _items.Add(stack, new StorageQuantityItem() { Origin = Origins[i] });
                stack.Changed += visualize;
                visualize(stack);
            }
        }

        private void visualize(ItemStack stack)
        {
            var item = _items[stack];

            if (stack.HasItems)
            {
                if (item.Visual == null)
                {
                    var visual = stack.Items.Item.Visuals.ElementAtOrDefault(VisualIndex);
                    if (visual != null)
                    {
                        item.Visual = Instantiate(visual, item.Origin);
                    }
                }

                item.Visual.SetQuantity((int)stack.Items.UnitQuantity);
            }
            else
            {
                if (item.Visual)
                {
                    Destroy(item.Visual.gameObject);
                    item.Visual = null;
                }
            }
        }
    }
}