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
        
        private async void OnGridClick(Grid.Grid grid)
        {
            if (_levelState.Value != LevelState.WaitingForInput)
            {
                return;
            }
            
            // do nothing if the grid is not occupied
            if (grid.Type != GridType.Occupied)
            {
                Debug.Log("Grid is not occupied, ignoring click");
                return;
            }
            
            var stickman = grid.GetStickmanInstance();
            var gridHumanType = grid.GetHumanType();
            
            if (stickman == null)
            {
                Debug.LogError("Stickman instance is null on occupied grid!");
                return;
            }
            
            // move to bus if types match
            if (gridHumanType == _currentHumanType)
            {
                Debug.Log($"Moving stickman to bus - Type: {gridHumanType}");
                await MoveStickmanToBus(stickman, grid);
            }
            // move to holder
            else
            {
                Debug.Log($"Moving stickman to holder - Grid Type: {gridHumanType}, Bus Type: {_currentHumanType}");
                await MoveStickmanToHolder(stickman, grid);
            }
        }
        
        private async UniTask MoveStickmanToBus(Stickman.Stickman stickman, Grid.Grid grid)
        {
            if (!busController.HasCurrentBus())
            {
                Debug.LogError("No current bus available!");
                return;
            }
            
            var currentBus = busController.GetCurrentBus();
            
            // Check if bus is full
            if (currentBus.IsFull())
            {
                Debug.Log("Current bus is full, getting next bus");
                _currentHumanType = await busController.GetNextBus();
                
                if (_currentHumanType == null)
                {
                    Debug.Log("Level completed!");
                    _levelState.SetValueAndForceNotify(LevelState.LevelCompleted);
                    return;
                }
                
                // Get the new bus
                currentBus = busController.GetCurrentBus();
                if (currentBus == null)
                {
                    Debug.LogError("No bus available after getting next bus!");
                    return;
                }
            }
            
            await stickman.MoveStickmanToPosition(currentBus.transform, 1f);
            currentBus.EnableNextStickman();
            ClearGrid(grid);
            
            // Only get next bus if current bus is now full
            if (currentBus.IsFull())
            {
                _currentHumanType = await busController.GetNextBus();
                
                if (_currentHumanType == null)
                {
                    Debug.Log("Level completed!");
                    _levelState.SetValueAndForceNotify(LevelState.LevelCompleted);
                }
                else
                {
                    // check holders for matching bus
                    await CheckHoldersForMatchingBus();
                }
            }
        }
        
        private async UniTask MoveStickmanToHolder(Stickman.Stickman stickman, Grid.Grid grid)
        {
            var holderTransform = holderController.GetNextEmptyHolderTransform();
            
            if (holderTransform == null)
            {
                Debug.LogError("No empty holder available!");
                return;
            }
            
            // move stickman to holder
            await stickman.MoveStickmanToPosition(holderTransform, 1f);
            
            var gridHumanType = grid.GetHumanType();
            holderController.FillNextEmptyHolderWithStickman(gridHumanType, stickman);
            
            // Clear grid but don't deactivate stickman (it's now in holder)
            ClearGridWithoutDeactivatingStickman(grid);
        }
        
        private async UniTask CheckHoldersForMatchingBus()
        {
            if (_currentHumanType == null)
            {
                return;
            }
            
            if (!busController.HasCurrentBus())
            {
                return;
            }
            
            var currentBus = busController.GetCurrentBus();
            var matchingHolders = holderController.GetHoldersWithHumanType(_currentHumanType.Value);
            
            // Only move stickmen up to bus capacity
            int stickmenToMove = Mathf.Min(matchingHolders.Count, DataConstants.BUS_MAX_STICMAN_COUNT - currentBus.GetCurrentStickmanCount());
            
            for (int i = 0; i < stickmenToMove; i++)
            {
                var holder = matchingHolders[i];
                var stickman = holder.GetStickmanInstance();
                if (stickman != null)
                {
                    Debug.Log($"Moving stickman from holder to bus - Type: {_currentHumanType}");
                    await MoveStickmanFromHolderToBus(stickman, holder);
                }
            }
        }
        
        private async UniTask MoveStickmanFromHolderToBus(Stickman.Stickman stickman, Holder.Holder holder)
        {
            if (!busController.HasCurrentBus())
            {
                Debug.LogError("No current bus available!");
                return;
            }
            
            var currentBus = busController.GetCurrentBus();
            
            // Check if bus is full before moving
            if (currentBus.IsFull())
            {
                Debug.Log("Bus is full, cannot move stickman from holder");
                return;
            }
            
            // move stickman to bus
            await stickman.MoveStickmanToPosition(currentBus.transform, 1f);
            
            currentBus.EnableNextStickman();
            holder.Vacate();
            
            // Only get next bus if current bus is now full
            if (currentBus.IsFull())
            {
                _currentHumanType = await busController.GetNextBus();
                
                if (_currentHumanType == null)
                {
                    Debug.Log("Level completed!");
                    _levelState.SetValueAndForceNotify(LevelState.LevelCompleted);
                }
                else
                {
                    await CheckHoldersForMatchingBus();
                }
            }
        }
        
        private void ClearGrid(Grid.Grid grid)
        {
            grid.SetType(GridType.Empty);
            
            var stickman = grid.GetStickmanInstance();
            if (stickman != null)
            {
                stickman.gameObject.SetActive(false);
            }
        }
        
        private void ClearGridWithoutDeactivatingStickman(Grid.Grid grid)
        {
            grid.SetType(GridType.Empty);
            // Don't deactivate stickman as it's moving to holder
        }
    }
}
