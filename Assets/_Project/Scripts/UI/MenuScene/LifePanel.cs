using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Utils;
using _Project.Scripts.Systems.Life;
using TMPro;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.UI.MenuScene
{
    public class LifePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lifeText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private ILifeManager _lifeManager;
        
        public void Initialize(UserConfig userConfig)
        {
            Locator.Instance.TryResolve(out _lifeManager);

            lifeText.text = userConfig.lifeCount.ToString();
            
            if (userConfig.lifeCount >= DataConstants.USER_MAX_LIFE_COUNT)
            {
                descriptionText.text = "FULL";
            }
            else
            {
                _lifeManager.RemainingTimeForNextLife.Subscribe(UpdateLifeTextAsATimer).AddTo(this);
            }
        }

        private void UpdateLifeTextAsATimer(int remainingTimeForNextLife)
        {
            descriptionText.text = remainingTimeForNextLife.ToMinuteSecond();
        }
    }
}
