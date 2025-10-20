using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.MenuScene
{
    public class GoldPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;

        public void Initialize(int goldCount)
        {
            goldText.text = goldCount.ToString();
        }
    }
}
