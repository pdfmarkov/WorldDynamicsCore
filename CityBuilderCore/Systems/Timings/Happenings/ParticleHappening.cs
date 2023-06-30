using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// an event that, during its activation, plays particles
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Happenings/" + nameof(ParticleHappening))]
    public class ParticleHappening : TimingHappening
    {
        [Tooltip("name of the gameobject that has a particle system that will play while the happening is active")]
        public string ObjectName;

        public override void Activate()
        {
            base.Activate();

            GameObject.Find(ObjectName).GetComponent<ParticleSystem>().Play();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            GameObject.Find(ObjectName).GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
