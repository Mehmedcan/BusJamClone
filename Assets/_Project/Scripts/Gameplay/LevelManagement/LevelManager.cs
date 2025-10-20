using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Data.ScriptableObjects.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Gameplay.Bus;
using _Project.Scripts.Gameplay.Grid;
using _Project.Scripts.Gameplay.Holder;
using _Project.Scripts.Gameplay.Human;
using _Project.Scripts.Systems.Save;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.Gameplay.LevelManagement
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] GridController gridController;
        [SerializeField] HolderController holderController;
        [SerializeField] BusController busController;
        
        // Level State Management
        private ReactiveProperty<LevelState> _levelState = new(LevelState.Idle);
        
        // Data
        private UserConfig _userConfig;
        private GameData _gameData;
        private LevelConfig _levelConfig;
        
        private HumanType? _currentHumanType;
        
        ISaveManager _saveManager;
        
        public void SetAndStartLevel()
        {
            SetupData();
            
            gridController.SetGridClick(OnGridClick);
            busController.Initialize(_levelConfig.busCount, _levelConfig.busHumanTypes);

            StartLevel().Forget();
        }
        
        private async UniTask StartLevel()
        {
            _currentHumanType = await busController.GetNextBus();
            
            _levelState.SetValueAndForceNotify(LevelState.WaitingForInput);

        }

        private void SetupData()
        {
            Locator.Instance.TryResolve(out _saveManager);
            
            _gameData = Resources.Load<GameData>(DataConstants.GAME_CONFIG_PATH);
            _userConfig = _saveManager.Load<UserConfig>(DataConstants.SAVE_KEY_USER_CONFIG);
            _levelConfig = _gameData.levels[_userConfig.level];
        }
        
        private void OnGridClick(Grid.Grid grid)
        {
            if (_levelState.Value != LevelState.WaitingForInput)
            {
                return;
            }
        }
    }
}
