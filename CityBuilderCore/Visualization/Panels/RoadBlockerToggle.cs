using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CityBuilderCore
{
    /// <summary>
    /// behaviour that can turn a tag on a <see cref="RoadBlockerComponent"/> on or off so that the related walkers can/cannot pass
    /// </summary>
    public class RoadBlockerToggle : MonoBehaviour
    {
        [Tooltip("toggle that determines if the tag can pass, its changed event has to be wired up in the inspector")]
        public Toggle Toggle;
        [Tooltip("text that displays the tags name")]
        public TMPro.TMP_Text Text;

        private RoadBlockerComponent _roadBlockerComponent;
        private KeyedObject _tag;

        public void Initialize(RoadBlockerComponent roadBlockerComponent, KeyedObject tag)
        {
            _roadBlockerComponent = roadBlockerComponent;
            _tag = tag;

            Toggle.SetIsOnWithoutNotify(!roadBlockerComponent.BlockedKeys.Contains(tag.Key));

            if (tag is WalkerInfo info)
                Text.text = info.Name;
            else
                Text.text = tag.name;
        }

        public void Changed(bool value)
        {
            if (_roadBlockerComponent == null)
                return;

            var blockedKeys = _roadBlockerComponent.BlockedKeys.ToList();

            if (value)
                blockedKeys.Remove(_tag.Key);
            else
                blockedKeys.Add(_tag.Key);

            _roadBlockerComponent.SetBlockedKeys(blockedKeys);
        }
    }
}