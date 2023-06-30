using UnityEngine;
using UnityEngine.Events;

namespace CityBuilderCore
{
    /// <summary>
    /// fires unity events when a key is pressed or released
    /// </summary>
    public class UnityKeyEvent : MonoBehaviour
    {
        public KeyCode Key;

        public UnityEvent KeyDown;
        public UnityEvent KeyUp;

        private void Update()
        {
            if (Input.GetKeyDown(Key))
                KeyDown?.Invoke();
            if (Input.GetKeyUp(Key))
                KeyUp?.Invoke();
        }
    }
}