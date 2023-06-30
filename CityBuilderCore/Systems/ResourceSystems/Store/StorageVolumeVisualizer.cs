using System.Collections.Generic;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// scales and colours meshes for the items stored in a stacked <see cref="IStorageComponent"/><br/>
    /// uses the material defined in <see cref="Item.Material"/>
    /// </summary>
    [RequireComponent(typeof(IStorageComponent))]
    public class StorageVolumeVisualizer : MonoBehaviour
    {
        [Tooltip("the renderers that will be scaled and have their material set to the items")]
        public MeshRenderer[] Volumes;
        [Tooltip("scale of volume when empty")]
        public Vector3 From;
        [Tooltip("scale of volume when full")]
        public Vector3 To;

        private IStorageComponent _storageComponent;
        private Dictionary<ItemStack, MeshRenderer> _volumeDict = new Dictionary<ItemStack, MeshRenderer>();

        private void Awake()
        {
            _storageComponent = GetComponent<IStorageComponent>();
        }

        private void Start()
        {
            for (int i = 0; i < _storageComponent.Storage.Stacks.Length; i++)
            {
                var stack = _storageComponent.Storage.Stacks[i];
                _volumeDict.Add(stack, Volumes[i]);
                stack.Changed += visualize;
                visualize(stack);
            }
        }

        private void visualize(ItemStack stack)
        {
            var renderer = _volumeDict[stack];

            if (stack.HasItems)
            {
                renderer.gameObject.SetActive(true);
                renderer.sharedMaterial = stack.Items.Item.Material;
                renderer.transform.localScale = Vector3.Lerp(From, To, stack.FillDegree);
            }
            else
            {
                renderer.gameObject.SetActive(false);
            }
        }
    }
}