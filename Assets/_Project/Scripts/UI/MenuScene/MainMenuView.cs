using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Scripts.Core;
using _Project.Scripts.Systems.Save;
using _Project.Scripts.Systems.SceneLoad;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.MenuScene
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private LifePanel lifePanel;
        [SerializeField] private GoldPanel goldPanel;
        [SerializeField] private Button playButton;

        private UserConfig _userConfig;
        
        private ISaveManager _saveManager;
        
        private const string GameScene = "GameScene";
        
        private void Awake()
        {
            Locator.Instance.TryResolve(out _saveManager);
            
            _userConfig = _saveManager.Load<UserConfig>(DataConstants.SAVE_KEY_USER_CONFIG);
            
            SetUI();
            SetPlayButton();
        }

        private void SetUI()
        {
            goldPanel.Initialize(goldCount: _userConfig.gold);
            lifePanel.Initialize(_userConfig);
        }

        private void SetPlayButton()
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private async void OnPlayButtonClicked()
        {
            var userConfig = _saveManager.Load<UserConfig>(DataConstants.SAVE_KEY_USER_CONFIG);
            if (userConfig.lifeCount <= 0)
            {
                playButton.enabled = false;
                return;
            }

            playButton.interactable = false;
            
            Locator.Instance.TryResolve<ISceneLoadManager>(out var sceneLoaderManager);
            await sceneLoaderManager.LoadSceneAsync(GameScene);
        }
    }
}
