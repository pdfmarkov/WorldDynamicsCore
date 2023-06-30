﻿using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// types of employment that can have different priorities so less essential groups loose access first(services, logistics, food, industry, ...)
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/" + nameof(EmploymentGroup))]
    public class EmploymentGroup : KeyedObject
    {
        [Tooltip("name of the group in the UI(logistics, services, food, ...)")]
        public string Name;
        [Tooltip(@"the order in which workers will be assigned(highest prio first)
this is used at the start of the game, it may be changed through the employment manager")]
        public int Priority;
    }
}