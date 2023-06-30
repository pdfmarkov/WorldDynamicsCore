using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// displays the current playtime in unity ui
    /// </summary>
    public class PlaytimeVisual : MonoBehaviour
    {
        [Tooltip("the ui element that gets its text set")]
        public TMPro.TMP_Text Text;
        [Tooltip("how to format the playtime when no units are used")]
        public string Format;
        [Tooltip("which units to use to display time")]
        public TimingUnit[] Units;

        private IGameSpeed _gameSpeed;

        private void Start()
        {
            _gameSpeed = Dependencies.Get<IGameSpeed>();
        }

        private void Update()
        {
            if (Units != null && Units.Length > 0)
            {
                Text.text = string.Join(" ", Units.Select(u => u.GetText(_gameSpeed.Playtime)));
            }
            else
            {
                Text.text = _gameSpeed.Playtime.ToString(Format);
            }
        }
    }
}