using System;
using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Data.ScriptableObjects.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Systems.Save;
using _Project.Scripts.Systems.Timer;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.Systems.Life
{
    public class UserDataManager : IUserDataManager
    {
        public string LogTag => "[UserDataManager]";
        public ReactiveProperty<int> RemainingTimeForNextLife { get; private set; }
        public event EventHandler<UserConfig> OnUserLifeChanged;
        
        private GameData _gameData;
        private IDisposable _timerDisposable;
        
        private ISaveManager _saveManager;
        private ITimeManager _timeManager;
        
        #region Base

        public UniTask AsyncInitialize()
        {
            return UniTask.CompletedTask;
        }

        public void Initialize()
        {
            Locator.Instance.TryResolve(out _timeManager);
            Locator.Instance.TryResolve(out _saveManager);
            
            LoadGameData();
            SetupLifeDataIfNeeded();
        }

        public void Dispose()
        {
            _timerDisposable?.Dispose();
            _timerDisposable = null;
            
            RemainingTimeForNextLife?.Dispose();
        }

        #endregion


        // --- Level Management ---
        
        public (UserConfig, int/*earned gold*/) WinLevel()
        {
            var userConfig = _saveManager.Load<UserConfig>(GameConstants.SAVE_KEY_USER_CONFIG);
            var levelData =_gameData.levels[ userConfig.level];

            var currentLevelGoldCount = levelData.goldCount;
            var nextLevel = userConfig.level + 1 > _gameData.levels.Count - 1 ? userConfig.level : userConfig.level + 1;
            
            var newUserConfig = new UserConfig
            {
                level = nextLevel,
                gold = userConfig.gold + currentLevelGoldCount,
                lifeCount = userConfig.lifeCount,
                lastFailTime = userConfig.lastFailTime
            };
            
            _saveManager.Save(GameConstants.SAVE_KEY_USER_CONFIG, newUserConfig);
            
            return (newUserConfig, currentLevelGoldCount);
        }
        
        public UserConfig LoseLevel()
        {
            ChangeUserLifeCount(-1);
            SetupLifeDataIfNeeded();
            
            var userConfig = _saveManager.Load<UserConfig>(GameConstants.SAVE_KEY_USER_CONFIG);
            return userConfig;
        }
        
        // --- Life Management ---
        private void LoadGameData()
        {
            _gameData = Resources.Load<GameData>(GameConstants.GAME_CONFIG_PATH);
        }

        public void SetupLifeDataIfNeeded()
        {
            RemainingTimeForNextLife ??= new ReactiveProperty<int>(0);

            var userConfig = _saveManager.Load<UserConfig>(GameConstants.SAVE_KEY_USER_CONFIG);
            if (userConfig.lifeCount >= GameConstants.USER_MAX_LIFE_COUNT)
            {
                return;
            }

            var userLifeRefillSeconds = _gameData.refillSeconds;
            var timeNow = DateTime.UtcNow;
            var elapsedTime = (timeNow - userConfig.lastFailTime).TotalSeconds;
            var refillCount = (int)(elapsedTime / userLifeRefillSeconds);
            var nextLifeRemainingSeconds = userLifeRefillSeconds - (elapsedTime % userLifeRefillSeconds);

            // max refilled
            if (refillCount >= (GameConstants.USER_MAX_LIFE_COUNT - userConfig.lifeCount))
            {
                ChangeUserLifeCount(GameConstants.USER_MAX_LIFE_COUNT);
                return;
            }
            else if (refillCount > 0)
            {
                ChangeUserLifeCount(refillCount);
            }

            _timerDisposable?.Dispose();
            _timerDisposable = _timeManager.StartTimer(seconds: (int)nextLifeRemainingSeconds, onSecond: OnLifeTimerTick, onCompleted: OnTimerCompleted);
        }

        private void OnLifeTimerTick(int remainingSeconds)
        {
            RemainingTimeForNextLife.SetValueAndForceNotify(remainingSeconds);
        }

        private void OnTimerCompleted()
        {
            ChangeUserLifeCount(changeAmount: 1);

            SetupLifeDataIfNeeded();
        }
        
        private void ChangeUserLifeCount(int changeAmount)
        {
            var userConfig = _saveManager.Load<UserConfig>(GameConstants.SAVE_KEY_USER_CONFIG);

            var isLifeIncreasing = changeAmount > 0;
            var isDecreaseFromFull = changeAmount < 0 && userConfig.lifeCount == GameConstants.USER_MAX_LIFE_COUNT;
            
            if (isLifeIncreasing || isDecreaseFromFull)
            {
                userConfig.lastFailTime = DateTime.UtcNow;
            }

            userConfig.lifeCount = Mathf.Min(userConfig.lifeCount + changeAmount, GameConstants.USER_MAX_LIFE_COUNT);
            _saveManager.Save(GameConstants.SAVE_KEY_USER_CONFIG, userConfig);
            
            OnUserLifeChanged?.Invoke(this, userConfig);
        }
    }
}
