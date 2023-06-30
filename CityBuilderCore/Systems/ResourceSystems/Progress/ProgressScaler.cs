using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// scales itself according to the attached <see cref="ProgressComponent"/>
    /// </summary>
    public class ProgressScaler : MonoBehaviour
    {
        [Tooltip("the building component that provides the progress ratio")]
        public ProgressComponent Component;
        [Tooltip("scale when progress is 0")]
        public Vector3 From = Vector3.zero;
        [Tooltip("scale when progress is full")]
        public Vector3 To = Vector3.one;

        private void Start()
        {
            Component.ProgressChanged += updateProgress;

            updateProgress(Component.Progress);
        }

        private void updateProgress(float progress)
        {
            transform.localScale = Vector3.Lerp(From, To, progress);
        }
    }
}