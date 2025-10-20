using System.Collections.Generic;
using _Project.Data.Constants;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Utils;
using _Project.Scripts.Gameplay.Human;
using _Project.Scripts.Systems.Pooling;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Bus
{
    public class BusController : MonoBehaviour
    {
        [Header("Bus Positions")]
        [SerializeField] private Transform busSpawnTransform;
        [SerializeField] private Transform busWaitTransform;
        [SerializeField] private Transform busMainTransform;
        [SerializeField] private Transform busEndTransform;
        
        // State Management
        private int _currentBusCount;
        private int _totalBusCount;
        private List<HumanType> _busHumanTypes;
        
        // Bus Management
        private Bus _mainBus;
        private Bus _waitingBus;
        
        private const int DelayBetweenBusSpawns = 100; // milliseconds
        
        private IPoolManager _poolManager;
        
        public void Initialize(int busCount, List<HumanType> busHumanTypes)
        {
            Locator.Instance.TryResolve(out _poolManager);

            _currentBusCount = 0;
            _totalBusCount = busCount;
            _busHumanTypes = busHumanTypes;
        }
        
        public async UniTask<HumanType?> GetNextBus()
        {
			// initial setup: spawn the first (main) bus, and optionally the waiting bus
			if (_currentBusCount == 0)
			{
				_mainBus = _poolManager.Get(GameConstants.BUS_POOL_KEY, transform).GetComponent<Bus>();
				await MoveBusToPosition(_mainBus, _busHumanTypes[_currentBusCount], busSpawnTransform, busMainTransform);
				_currentBusCount++;

				if (_totalBusCount > 1)
				{
					await UniTask.Delay(DelayBetweenBusSpawns);
					_waitingBus = _poolManager.Get(GameConstants.BUS_POOL_KEY, transform).GetComponent<Bus>();
					await MoveBusToPosition(_waitingBus, _busHumanTypes[_currentBusCount], busSpawnTransform, busWaitTransform);
					_currentBusCount++;
				}
				return _busHumanTypes[0]; // Return the first bus type
			}

			// already finished, do nothing
			if (_mainBus == null)
			{
				return null;
			}

			var mainIndex = Mathf.Clamp(_currentBusCount - 2, 0, _busHumanTypes.Count - 1);
			var waitingIndex = Mathf.Clamp(_currentBusCount - 1, 0, _busHumanTypes.Count - 1);

			// case 1: there is a waiting bus
			if (_waitingBus != null)
			{
				await MoveBusToPosition(_mainBus, _busHumanTypes[mainIndex], busMainTransform, busEndTransform); // main -> end
				
				await MoveBusToPosition(_waitingBus, _busHumanTypes[waitingIndex], busWaitTransform, busMainTransform); // waiting -> main
				_mainBus = _waitingBus;
				_waitingBus = null;

				// if there are more buses to spawn, bring a new one to wait
				if (_currentBusCount < _totalBusCount)
				{
					await UniTask.Delay(DelayBetweenBusSpawns);
					_waitingBus = _poolManager.Get(GameConstants.BUS_POOL_KEY, transform).GetComponent<Bus>();
					await MoveBusToPosition(_waitingBus, _busHumanTypes[_currentBusCount], busSpawnTransform, busWaitTransform);
					_currentBusCount++;
				}

				return _busHumanTypes[waitingIndex]; // Return the new main bus type
			}

			// Case 2: No waiting bus remains. Move the last main bus to end and finish.
			await MoveBusToPosition(_mainBus, _busHumanTypes[^1], busMainTransform, busEndTransform);
			_mainBus = null;
			
			return null; // No more buses
        }
        
        private UniTask MoveBusToPosition(Bus bus, HumanType humanType, Transform currentTransform, Transform targetTransform)
        {
	        bus.transform.position = currentTransform.position;
	        bus.gameObject.SetActive(true);

	        var busNeedsInitialization = currentTransform == busSpawnTransform;
	        if(busNeedsInitialization) bus.Initialize(humanType);
            
	        return bus.transform.DOMove(targetTransform.position, 1f).SetEase(Ease.InOutSine).ToUniTask();
        } 

        public Bus GetCurrentBus()
        {
            return _mainBus;
        }
        
        public bool HasCurrentBus()
        {
            return _mainBus != null;
        } 

    }
}
