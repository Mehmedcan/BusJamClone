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
    public class LifeManager : ILifeManager
    {
        public string LogTag => "[LifeManager]";
        public ReactiveProperty<int> RemainingTimeForNextLife { get; private set; }

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


        private void LoadGameData()
        {
            _gameData = Resources.Load<GameData>(DataConstants.GAME_CONFIG_PATH);
        }

        public void SetupLifeDataIfNeeded()
        {
            RemainingTimeForNextLife = new ReactiveProperty<int>();
            
            var userConfig = _saveManager.Load<UserConfig>(DataConstants.SAVE_KEY_USER_CONFIG);
            if (userConfig.lifeCount >= DataConstants.USER_MAX_LIFE_COUNT)
            {
                return;
            }

            var userLifeRefillSeconds = _gameData.refillSeconds;
            var timeNow = DateTime.UtcNow;
            var elapsedTime = (timeNow - userConfig.lastFailTime).TotalSeconds;
            var refillCount = (int)(elapsedTime / userLifeRefillSeconds);
            var nextLifeRemainingSeconds = userLifeRefillSeconds - (elapsedTime % userLifeRefillSeconds);

            // max refilled
            if (refillCount >= (DataConstants.USER_MAX_LIFE_COUNT - userConfig.lifeCount))
            {
                IncreaseUserLifeCount(DataConstants.USER_MAX_LIFE_COUNT);
                return;
            }
            else if (refillCount > 0)
            {
                IncreaseUserLifeCount(refillCount);
                return;
            }
            else
            {
                _timerDisposable?.Dispose();
                _timerDisposable = _timeManager.StartTimer(seconds : (int)nextLifeRemainingSeconds, onSecond: OnLifeTimerTick, onCompleted: null);
            }
        }

        private void OnLifeTimerTick(int remainingSeconds)
        {
            RemainingTimeForNextLife.SetValueAndForceNotify(remainingSeconds);
        }
        
        private void IncreaseUserLifeCount(int increaseCount = 1)
        {
            var userConfig = _saveManager.Load<UserConfig>(DataConstants.SAVE_KEY_USER_CONFIG);
          
            userConfig.lifeCount += increaseCount;
           
            if (userConfig.lifeCount >= DataConstants.USER_MAX_LIFE_COUNT) // to set max life intialy
            {
                userConfig.lastFailTime = DateTime.UtcNow;
            }
            
            _saveManager.Save(DataConstants.SAVE_KEY_USER_CONFIG, userConfig);
        }
    }
}
