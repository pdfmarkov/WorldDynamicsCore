using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// proxy for setting animator values from unityevents
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class UnityAnimatorEventVector : MonoBehaviour
    {
        public string Parameter;
        public bool SetX;
        public bool SetY;
        public bool SetZ;

        private int _hashX, _hashY, _hashZ;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (SetX)
                _hashX = Animator.StringToHash(Parameter + "X");
            if (SetY)
                _hashY = Animator.StringToHash(Parameter + "Y");
            if (SetX)
                _hashZ = Animator.StringToHash(Parameter + "Z");
        }

        public void SetVector3(Vector3 vector)
        {
            if (SetX)
                _animator.SetFloat(_hashX, vector.x);
            if (SetY)
                _animator.SetFloat(_hashY, vector.y);
            if (SetZ)
                _animator.SetFloat(_hashZ, vector.z);
        }
    }
}