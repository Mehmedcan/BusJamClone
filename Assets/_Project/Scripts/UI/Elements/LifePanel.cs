using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Utils;
using _Project.Scripts.Systems.Life;
using TMPro;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.UI.Elements
{
    public class LifePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lifeText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private IUserDataManager _userDataManager;
        
        public void Initialize(UserConfig userConfig)
        {
            Locator.Instance.TryResolve(out _userDataManager);

            lifeText.text = userConfig.lifeCount.ToString();
            if (userConfig.lifeCount >= GameConstants.USER_MAX_LIFE_COUNT)
            {
                descriptionText.text = "FULL";
            }
            else
            {
                _userDataManager.RemainingTimeForNextLife.Subscribe(UpdateLifeTextAsATimer).AddTo(this);
            }
            
            _userDataManager.OnUserLifeChanged += OnUserLifeChanged;
        }

        private void UpdateLifeTextAsATimer(int remainingTimeForNextLife)
        {
            descriptionText.text = remainingTimeForNextLife.ToMinuteSecond();
        }

        private void OnUserLifeChanged(object sender, UserConfig e)
        {
            lifeText.text = e.lifeCount.ToString();
            
            if (e.lifeCount >= GameConstants.USER_MAX_LIFE_COUNT)
            {
                descriptionText.text = "FULL";
            }
        }
      
        private void OnDestroy()
        {
            if (_userDataManager != null)
            {
                _userDataManager.OnUserLifeChanged -= OnUserLifeChanged;
            }
        }
    }
}
