﻿using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// behaviour that constantly sets an animator to the current progress of a timing unit
    /// check out the Lights in THREE for an example
    /// </summary>
    public class TimingAnimationLoop : MonoBehaviour
    {
        [Tooltip("the animator that will have its time set")]
        public Animator Animator;
        [Tooltip("animation state name")]
        public string StateName;
        [Tooltip("animation layer id")]
        public int Layer;
        [Tooltip("the timing unit that will provide the time")]
        public TimingUnit Unit;

        private void Update()
        {
            Animator.Play(StateName, Layer, Unit.GetRatio(Dependencies.Get<IGameSpeed>().Playtime));
        }
    }
}
