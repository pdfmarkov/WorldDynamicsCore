using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// proxy for setting animator values from unityevents
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class UnityAnimatorEvent : MonoBehaviour
    {
        public string Parameter;

        private int _hash;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _hash = Animator.StringToHash(Parameter);
        }

        public void SetBool(bool value) => _animator.SetBool(_hash, value);
        public void SetInteger(int value) => _animator.SetInteger(_hash, value);
        public void SetFloat(float value) => _animator.SetFloat(_hash, value);
    }
}