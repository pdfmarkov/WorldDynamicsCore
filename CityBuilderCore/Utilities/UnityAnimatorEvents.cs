using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// proxy for setting animator values from unityevents
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class UnityAnimatorEvents : MonoBehaviour
    {
        public string Parameter1;
        public string Parameter2;
        public string Parameter3;
        public string Parameter4;
        public string Parameter5;

        private int _hash1;
        private int _hash2;
        private int _hash3;
        private int _hash4;
        private int _hash5;

        private Animator _animator;
        public Animator Animator
        {
            get
            {
                if (_animator == null)
                    init();
                return _animator;
            }
        }

        private void init()
        {
            _animator = GetComponent<Animator>();
            _hash1 = Animator.StringToHash(Parameter1);
            _hash2 = Animator.StringToHash(Parameter2);
            _hash3 = Animator.StringToHash(Parameter3);
            _hash4 = Animator.StringToHash(Parameter4);
            _hash5 = Animator.StringToHash(Parameter5);
        }

        public void SetBool1(bool value) => Animator.SetBool(_hash1, value);
        public void SetInteger1(int value) => Animator.SetInteger(_hash1, value);
        public void SetFloat1(float value) => Animator.SetFloat(_hash1, value);

        public void SetBool2(bool value) => Animator.SetBool(_hash2, value);
        public void SetInteger2(int value) => Animator.SetInteger(_hash2, value);
        public void SetFloat2(float value) => Animator.SetFloat(_hash2, value);

        public void SetBool3(bool value) => Animator.SetBool(_hash3, value);
        public void SetInteger3(int value) => Animator.SetInteger(_hash3, value);
        public void SetFloat3(float value) => Animator.SetFloat(_hash3, value);

        public void SetBool4(bool value) => Animator.SetBool(_hash4, value);
        public void SetInteger4(int value) => Animator.SetInteger(_hash4, value);
        public void SetFloat4(float value) => Animator.SetFloat(_hash4, value);

        public void SetBool5(bool value) => Animator.SetBool(_hash5, value);
        public void SetInteger5(int value) => Animator.SetInteger(_hash5, value);
        public void SetFloat5(float value) => Animator.SetFloat(_hash5, value);
    }
}