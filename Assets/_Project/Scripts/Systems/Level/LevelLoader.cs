using _Project.Data.Constants;
using _Project.Data.GameData;
using _Project.Data.ScriptableObjects.Data;
using _Project.Scripts.Core;
using _Project.Scripts.Gameplay.Grid;
using _Project.Scripts.Gameplay.Holder;
using _Project.Scripts.Gameplay.LevelManagement;
using _Project.Scripts.Systems.Pooling;
using _Project.Scripts.Systems.Save;
using UnityEngine;

namespace _Project.Scripts.Systems.Level
{
    public class LevelLoader : MonoBehaviour
    { 
       [Header("Data")]
       [SerializeField] GameData gameData;

       [Space] [Header("Level Design")]
       [SerializeField] private HolderController holderController;
       [SerializeField] private GridController gridController;

       [Space] [Header("Level Objects")]
       [SerializeField] private GameObject stickmanPrefab;
       [SerializeField] private GameObject busPrefab;
       
       [Space] [Header("Level Management")]
       [SerializeField] private LevelManager levelManager;
       
       private ISaveManager _saveManager;
       private IPoolManager _poolManager;
       
       private const int StickmanPoolInitialSize = 20;
       private const int BusPoolInitialSize = 3;
       
       private void Awake()
       {
           Locator.Instance.TryResolve(out _saveManager);
           Locator.Instance.TryResolve(out _poolManager);

           LoadLevel();
           levelManager.SetAndStartLevel();
       }
       
       private void LoadLevel()
       {
           var userData = _saveManager.Load<UserConfig>(GameConstants.SAVE_KEY_USER_CONFIG);
           var levelData = gameData.levels[userData.level];
           
           _poolManager.CreatePool(GameConstants.STICKMAN_POOL_KEY, stickmanPrefab, StickmanPoolInitialSize);
           _poolManager.CreatePool(GameConstants.BUS_POOL_KEY, busPrefab, BusPoolInitialSize);
           
           var gridSizeData = new Vector2Int(levelData.width, levelData.height);
           gridController.CreateGridBoard(gridSizeData, levelData.cells, 0.1f);

           var holderCount = levelData.holderCount;
           holderController.CreateHolders(holderCount);
       }
    }
}
