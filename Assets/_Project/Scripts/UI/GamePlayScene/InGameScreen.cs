using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Scripts.Core;
using _Project.Scripts.Systems.Save;
using _Project.Scripts.UI.MenuScene;
using UnityEngine;

namespace _Project.Scripts.UI.GamePlayScene
{
    public class InGameScreen : MonoBehaviour
    {
        [SerializeField] private GoldPanel goldPanel;
        [SerializeField] private LevelPanel levelPanel;

        private UserConfig _userConfig;
        
        private ISaveManager _saveManager; 
        
        private void Awake()
        {
            Locator.Instance.TryResolve(out _saveManager);
            _userConfig = _saveManager.Load<UserConfig>(DataConstants.SAVE_KEY_USER_CONFIG);
            
            SetUI();
        }

        private void SetUI()
        {
            goldPanel.Initialize(goldCount: _userConfig.gold);
            levelPanel.Initialize(level: _userConfig.level);
        }
    }
}
