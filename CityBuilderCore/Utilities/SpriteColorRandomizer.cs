using UnityEngine;

namespace CityBuilderCore
{
    public class SpriteColorRandomizer:MonoBehaviour
    {
        public SpriteRenderer[] SpriteRenderers;
        public Color[] Colors;

        private void Start()
        {
            var color = Colors.Random();
            SpriteRenderers.ForEach(r => r.color = color);
        }
    }
}
