using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// used by <see cref="LayerKeyVisualizer"/> to display the value for an affector
    /// </summary>
    public class LayerAffectorVisualizer : MonoBehaviour
    {
        [Tooltip("name of the affector will be displayed here")]
        public TMPro.TMP_Text NameText;
        [Tooltip("the value of the affector will be displayed here")]
        public TMPro.TMP_Text ValueText;
    }
}
