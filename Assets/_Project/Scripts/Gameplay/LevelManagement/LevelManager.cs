using System.Collections.Generic;
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
using TMPro;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.Gameplay.LevelManagement
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] GridController gridController;
        [SerializeField] HolderController holderController;
        [SerializeField] BusController busController;
        
        [Space] [Header("Debug Screen")]
        [SerializeField] TextMeshProUGUI debugText;
        
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
            SetupDebugging();
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
            
            if (grid.Type != GridType.Occupied) // do nothing if the grid is not occupied
            {
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
            
            if (currentBus.IsFull())
            {
                Debug.Log("Current bus is full, getting next bus");
                _currentHumanType = await busController.GetNextBus();
                
                if (_currentHumanType == null)
                {
                    _levelState.SetValueAndForceNotify(LevelState.LevelCompleted);
                    return;
                }
                
                currentBus = busController.GetCurrentBus();
                if (currentBus == null)
                {
                    Debug.LogError("No bus available after getting next bus!");
                    return;
                }
            }
            
            await stickman.MoveStickmanToPosition(currentBus.transform, 1f);
            currentBus.EnableNextStickman();
            grid.ClearGrid();
            
            // Only get next bus if current bus is now full
            if (currentBus.IsFull())
            {
                _currentHumanType = await busController.GetNextBus();
                
                if (_currentHumanType == null)
                {
                    _levelState.SetValueAndForceNotify(LevelState.LevelCompleted);
                }
                else
                {
                    await CheckHoldersForMatchingBus();
                    CheckForFailCondition();
                }
            }
        }
        
        private async UniTask MoveStickmanToHolder(Stickman.Stickman stickman, Grid.Grid grid)
        {
            var gridHumanType = grid.GetHumanType();
            var holder = holderController.GetAndFillNextEmptyHolder(gridHumanType, stickman);
            
            if (holder == null)
            {
                Debug.LogError("No empty holder available!");
                CheckForFailCondition();
                return;
            }
            
            await stickman.MoveStickmanToPosition(holder.transform, 1f);
            grid.ClearGrid(shouldDeactiveStickman: false);
            
            // check for fail condition after moving to holder
            CheckForFailCondition();
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
            
            var stickmanCountToMove = Mathf.Min(matchingHolders.Count, DataConstants.BUS_MAX_STICMAN_COUNT - currentBus.GetCurrentStickmanCount());
            
            var moveTasks = new List<UniTask>();
            for (var i = 0; i < stickmanCountToMove; i++)
            {
                var holder = matchingHolders[i];
                var stickman = holder.GetStickmanInstance();
                if (stickman != null)
                {
                    var moveTask = MoveStickmanFromHolderToBus(stickman, holder);
                    moveTasks.Add(moveTask);
                }
            }
            
            await UniTask.WhenAll(moveTasks);
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
            
            // Enable the bus's own sitting stickman and deactivate the holder's stickman
            currentBus.EnableNextStickman();
            stickman.gameObject.SetActive(false);
            
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
                    // Check for fail condition after bus change
                    CheckForFailCondition();
                }
            }
        }
        
        private void CheckForFailCondition()
        {
            // all holders are full AND the current bus type has no matching holders
           
            if (holderController.AreAllHoldersFull() && _currentHumanType.HasValue)
            {
                bool hasMatchingHolder = holderController.HasHolderWithHumanType(_currentHumanType.Value);
                
                if (!hasMatchingHolder)
                {
                    Debug.LogError("Level Failed! All holders are full and no matching stickman for current bus type.");
                    _levelState.SetValueAndForceNotify(LevelState.LevelFailed);
                }
            }
        }
        
        private void SetupDebugging()
        {
            _levelState.Subscribe(state =>
            {
                debugText.text = $"Level State: {state}\n" +
                                 $"Current Bus Type: {_currentHumanType}\n" +
                                 $"Empty Holders: {holderController.GetEmptyHolderCount()}\n" +
                                 $"All Holders Full: {holderController.AreAllHoldersFull()}";
            }).AddTo(this);
        }

    }
}
