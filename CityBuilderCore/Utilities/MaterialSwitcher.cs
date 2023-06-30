using UnityEngine;

namespace CityBuilderCore
{
    public class MaterialSwitcher : MonoBehaviour
    {
        public Material MaterialA;
        public Material MaterialB;
        public MeshRenderer Target;
        public int TargetIndex;

        public void SetMaterial(bool value)
        {
            var materials = Target.sharedMaterials;
            materials[TargetIndex]= value ? MaterialA : MaterialB;
            Target.sharedMaterials = materials;
        }
    }
}
