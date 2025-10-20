using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.Elements
{
    public class LevelPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;

        public void Initialize(int level)
        {
            levelText.text = $"LEVEL {level + 1}";
        }
    }
}
