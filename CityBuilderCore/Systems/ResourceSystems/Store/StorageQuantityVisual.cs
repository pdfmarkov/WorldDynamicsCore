﻿using UnityEngine;

namespace CityBuilderCore
{
    /// <summary>
    /// visualizes item quantity by setting gameObjects activity
    /// </summary>
    public class StorageQuantityVisual : MonoBehaviour
    {
        [Tooltip("true > show only object[quantity-1] | false > show object up to quantity")]
        public bool Swap;
        [Tooltip("all the different visuals, count should be equal capacity")]
        public GameObject[] Objects;

        public void SetQuantity(int quantity)
        {
            for (int i = 0; i < Objects.Length; i++)
            {
                if (Swap)
                    Objects[i].SetActive(i == quantity - 1);
                else
                    Objects[i].SetActive(i < quantity);
            }
        }
    }
}