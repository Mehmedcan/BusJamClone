using System.Collections.Generic;
using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Data.ScriptableObjects.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Gameplay.Bus;
using _Project.Scripts.Gameplay.Grid;
using _Project.Scripts.Gameplay.Holder;
using _Project.Scripts.Gameplay.Human;
using _Project.Scripts.Systems.Life;
using _Project.Scripts.Systems.Save;
using _Project.Scripts.UI.Views;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.Gameplay.LevelManagement
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GridController gridController;
        [SerializeField] private HolderController holderController;
        [SerializeField] private BusController busController;
        
        [Space] [Header("UI Screen Variables")]
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private GamePlayView gamePlayView;
        
        [Space] [Header("Debug Mode (for me)")]
        [SerializeField] private bool isDebugMode = false;
        
        // Level State Management
        private ReactiveProperty<LevelState> _levelState = new(LevelState.Idle);
        
        // Data
        private UserConfig _userConfig;
        private GameData _gameData;
        private LevelConfig _levelConfig;
        
        private HumanType? _currentHumanType;

        private IUserDataManager _userDataManager;
        private ISaveManager _saveManager;
        
        public void SetAndStartLevel()
        {
            SetupDebugging();
            SetupData();
            
            gridController.SetGridClick(OnGridClick);
            busController.Initialize(_levelConfig.busCount, _levelConfig.busHumanTypes);

            StartLevel().Forget();
        }
        
        private void SetupData()
        {
            Locator.Instance.TryResolve(out _saveManager);
            Locator.Instance.TryResolve(out _userDataManager);
            
            _gameData = Resources.Load<GameData>(GameConstants.GAME_CONFIG_PATH);
            _userConfig = _saveManager.Load<UserConfig>(GameConstants.SAVE_KEY_USER_CONFIG);
            _levelConfig = _gameData.levels[_userConfig.level];
        }

        private async UniTask StartLevel()
        {
            _currentHumanType = await busController.GetNextBus();
            
            _levelState.SetValueAndForceNotify(LevelState.WaitingForInput);
        }
        
        
        private async void OnGridClick(Grid.Grid grid)
        {
            if (_levelState.Value != LevelState.WaitingForInput)
            {
                return;
            }
            
            if (grid.Type != GridType.Occupied)
            {
                return;
            }

            var canStickmanExit = gridController.CanStickmanExit(grid);
            if (!canStickmanExit)
            {
                return;
            }
            
            var stickman = grid.GetStickmanInstance();
            var gridHumanType = grid.GetHumanType();
            
            if (stickman == null)
            {
                return;
            }
            
            if (gridHumanType == _currentHumanType)
            {
                var currentBus = busController.GetCurrentBus();
                if (currentBus != null && !currentBus.CanAcceptStickman())
                {
                    _levelState.SetValueAndForceNotify(LevelState.WaitingForBus);
                    return;
                }
            }
            
            grid.SetType(GridType.Processing);
            
            try
            {
                // move to bus
                if (gridHumanType == _currentHumanType)
                {
                    await MoveStickmanToBus(stickman, grid);
                }
                // move to holder
                else
                {
                    await MoveStickmanToHolder(stickman, grid);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing grid click: {ex.Message}");
                grid.SetType(GridType.Occupied);
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
            
            currentBus.ReserveStickmanSlot();
            await stickman.MoveStickmanToPosition(currentBus.transform);
            
            currentBus.ConfirmStickmanArrival();
            currentBus.EnableNextStickman();
            
            grid.ClearGrid();
            
            if (currentBus.IsFull())
            {
                _levelState.SetValueAndForceNotify(LevelState.WaitingForBus);
                
                _currentHumanType = await busController.GetNextBus();
                
                if (_currentHumanType == null)
                {
                    FinishLevel(isWin: true);
                }
                else
                {
                    await CheckHoldersForMatchingBus();
                    CheckForFailCondition();
                    
                    _levelState.SetValueAndForceNotify(LevelState.WaitingForInput);
                }
            }
            else
            {
                _levelState.SetValueAndForceNotify(LevelState.WaitingForInput);
            }
        }
        
        private async UniTask MoveStickmanToHolder(Stickman.Stickman stickman, Grid.Grid grid)
        {
            var gridHumanType = grid.GetHumanType();
            var holder = holderController.GetAndFillNextEmptyHolder(gridHumanType, stickman);
            
            if (holder == null)
            {
                CheckForFailCondition();
                return;
            }
            
            await stickman.MoveStickmanToPosition(holder.transform);
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
            
            var stickmanCountToMove = Mathf.Min(matchingHolders.Count, GameConstants.BUS_MAX_STICMAN_COUNT - currentBus.GetCurrentStickmanCount());
            
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
            
            if (currentBus.IsFull())
            {
                Debug.Log("Bus is full, cannot move stickman from holder");
                return;
            }
            

            await stickman.MoveStickmanToPosition(currentBus.transform);
            
            currentBus.EnableNextStickman();
            stickman.gameObject.SetActive(false);
            
            holder.Vacate();
            
            // if current is full, get next bus
            if (currentBus.IsFull())
            {
                _levelState.SetValueAndForceNotify(LevelState.WaitingForBus);
                
                _currentHumanType = await busController.GetNextBus();
                
                if (_currentHumanType == null)
                {
                    FinishLevel(isWin: true);
                }
                else
                {
                    await CheckHoldersForMatchingBus();
                    
                    CheckForFailCondition();
                    
                    _levelState.SetValueAndForceNotify(LevelState.WaitingForInput);
                }
            }
        }
        
        private void CheckForFailCondition()
        {
            var isAllHoldersFull = holderController.AreAllHoldersFull();
            var currentBusExists = _currentHumanType.HasValue;
           
            if (isAllHoldersFull && currentBusExists)
            {
                var hasMatchingHolder = holderController.HasHolderWithHumanType(_currentHumanType.Value);
                
                if (!hasMatchingHolder)
                {
                    FinishLevel(isWin: false);
                }
            }
        }

        private void FinishLevel(bool isWin)
        {
            if(_levelState.Value is LevelState.LevelCompleted or LevelState.LevelFailed) return;
            
            var levelStateType = isWin ? LevelState.LevelCompleted : LevelState.LevelFailed;
            _levelState.SetValueAndForceNotify(levelStateType);
            
            if (isWin)
            {
                var winData = _userDataManager.WinLevel();
                gamePlayView.ShowWinScreen(userConfig: winData.Item1, earnedGold: winData.Item2);
            }
            else
            {
                var newUserConfig = _userDataManager.LoseLevel();
                gamePlayView.ShowLoseScreen(newUserConfig);
            }
            
            gridController.ReturnPoolObjects();
            busController.ReturnPoolObjects();
        }
        
        private void SetupDebugging()
        {
            gamePlayView.SetDebugView(isDebugMode);
            
            if (isDebugMode)
            {
                Observable.EveryUpdate().Subscribe(_ =>
                {
                    debugText.text = $"Level State: {_levelState.Value}\n" +
                                     $"Current Bus Type: {_currentHumanType}\n" +
                                     $"Empty Holders: {holderController.GetEmptyHolderCount()}\n" +
                                     $"All Holders Full: {holderController.AreAllHoldersFull()}";
                }).AddTo(this);
            }
        }

    }
}
