using _Project.Data.GameData;
using _Project.Scripts.Core;
using _Project.Scripts.Systems.SceneLoad;
using _Project.Scripts.UI.MenuScene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.GamePlayScene
{
    public class WinScreen : MonoBehaviour
    {
        [SerializeField] private GoldPanel goldPanel;
        [SerializeField] private TextMeshProUGUI earnedGoldText;
        [SerializeField] private Button continueButton;
        
        private const string GameScene = "GameScene";
        
        public void Initialize(UserConfig userConfig, int earnedGold)
        {
            goldPanel.Initialize(userConfig.gold);
            earnedGoldText.text = earnedGold.ToString();
            
            SetContinueButton();
        }
        
        private void SetContinueButton()
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        private async void OnContinueButtonClicked()
        {
            continueButton.interactable = false;
            
            Locator.Instance.TryResolve<ISceneLoadManager>(out var sceneLoaderManager);
            await sceneLoaderManager.LoadSceneAsync(GameScene);
        }
    }
}