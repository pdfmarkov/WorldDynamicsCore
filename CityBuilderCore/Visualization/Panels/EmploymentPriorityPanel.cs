using System;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// unity ui panel for switching around the priorities of <see cref="EmploymentGroup"/>
    /// </summary>
    public class EmploymentPriorityPanel : MonoBehaviour
    {
        [Tooltip("will display the name of the employment group")]
        public TMPro.TMP_Text NameText;
        [Tooltip("object that gets deactivated on the top element(up arrow)")]
        public GameObject UpObject;
        [Tooltip("object that gets deactivated on the bottom element(down arrow)")]
        public GameObject DownObject;

        public EmploymentGroup EmploymentGroup { get; private set; }

        public event Action<EmploymentPriorityPanel> Uped;
        public event Action<EmploymentPriorityPanel> Downed;

        public void Initialize(EmploymentGroup employmentGroup)
        {
            EmploymentGroup = employmentGroup;

            NameText.text = employmentGroup.Name;
        }

        public void SetVisibilities(bool isFirst, bool isLast)
        {
            UpObject.SetActive(!isFirst);
            DownObject.SetActive(!isLast);
        }

        public void Up() => Uped?.Invoke(this);
        public void Down() => Downed?.Invoke(this);
    }
}