﻿using System.Linq;
using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// sums other scores together
    /// </summary>
    [CreateAssetMenu(menuName = "CityBuilder/Scores/" + nameof(SummedScore))]
    public class SummedScore : Score
    {
        [Tooltip("flat value that will be added to the score")]
        public int BaseValue;
        [Tooltip("the other scored that will be added together")]
        public Score[] Scores;

        public override int Calculate() => BaseValue + Scores.Sum(s => s.Calculate());
    }
}