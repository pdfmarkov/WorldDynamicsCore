using UnityEngine;

namespace CityBuilderUrban
{
    public class MoneyVisual : MonoBehaviour
    {
        public TMPro.TMP_Text Text;

        public void Set(int quantity)
        {
            if (quantity > 0)
                Text.color = Color.green;
            else
                Text.color = Color.red;

            Text.text = quantity.ToString();
        }
        public void Done() => Destroy(gameObject);
    }
}
