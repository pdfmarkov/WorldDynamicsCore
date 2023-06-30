
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// visualizes employment as text in unity ui<br/>
    /// </summary>
    public class PopulationVisualizer : MonoBehaviour
    {
        public Population Population;
        public TMPro.TMP_Text Text;

        private IPopulationManager _populationManager;

        private void Start()
        {
            _populationManager = Dependencies.Get<IPopulationManager>();
        }

        private void Update()
        {
            Text.text = $"{Population.Name}: {_populationManager.GetQuantity(Population)} / {_populationManager.GetCapacity(Population)}";
        }
    }
}