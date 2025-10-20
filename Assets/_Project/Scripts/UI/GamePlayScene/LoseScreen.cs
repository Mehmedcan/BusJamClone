using _Project.Data.GameData;
using _Project.Scripts.Core;
using _Project.Scripts.Systems.SceneLoad;
using _Project.Scripts.UI.MenuScene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.GamePlayScene
{
    public class LoseScreen : MonoBehaviour
    {
        [SerializeField] private GoldPanel goldPanel;
        [SerializeField] private LifePanel lifePanel;
        [SerializeField] private TextMeshProUGUI retryButtonText;
        [SerializeField] private Button retryButton;
        
        UserConfig _userConfig;
        
        private const string MenuScene = "MenuScene";
        private const string GameScene = "GameScene";
        
        public void Initialize(UserConfig userConfig)
        {
            _userConfig = userConfig;
            
            goldPanel.Initialize(_userConfig.gold);
            lifePanel.Initialize(_userConfig);
            
            SetRetryButton();
        }

        private void SetRetryButton()
        {
            var buttonText = _userConfig.lifeCount > 0 ? "RETRY" : "MENU";
            retryButtonText.text = buttonText;
            
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        private void OnRetryButtonClicked()
        {
            Locator.Instance.TryResolve<ISceneLoadManager>(out var sceneLoaderManager);
            
            if (_userConfig.lifeCount > 0)
            {
                sceneLoaderManager.LoadSceneAsync(GameScene);
            }
            else
            {
                sceneLoaderManager.LoadSceneAsync(MenuScene);
            }
        }
    }
}