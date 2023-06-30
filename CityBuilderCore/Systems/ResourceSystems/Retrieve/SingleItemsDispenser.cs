using UnityEngine;
using UnityEngine.Events;

namespace CityBuilderCore
{
    /// <summary>
    /// items dispenser that dispenses once and then self destructs
    /// </summary>
    public class SingleItemsDispenser : MonoBehaviour, IItemsDispenser
    {
        [Tooltip("dispenser key, used in retrievers")]
        public string Key;
        [Tooltip("items returned on dispense")]
        public ItemQuantity Items;

        [Tooltip("fired when the dispenser is used")]
        public UnityEvent Dispensed;

        public Vector3 Position => transform.position;
        string IItemsDispenser.Key => Key;

        private void Start()
        {
            Dependencies.Get<IItemsDispenserManager>().Add(this);
        }

        private void OnDestroy()
        {
            Dependencies.Get<IItemsDispenserManager>().Remove(this);
        }

        public ItemQuantity Dispense()
        {
            Dispensed?.Invoke();
            Destroy(gameObject);

            return Items;
        }
    }
}