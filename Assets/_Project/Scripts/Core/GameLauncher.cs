using _Project.Scripts.Systems.Life;
using _Project.Scripts.Systems.Pooling;
using _Project.Scripts.Systems.Save;
using _Project.Scripts.Systems.SceneLoad;
using _Project.Scripts.Systems.Timer;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class GameLauncher : MonoBehaviour
    {
        private const string MenuScene = "MenuScene";
        
        private void Awake()
        {
            LaunchAsync().Forget();
        }

        private async UniTask LaunchAsync()
        {
            await InitAllManagers();
            await InitLifeManager();

            await UniTask.WaitForSeconds(duration: 1f); // Simulate some async loading time
            
            Locator.Instance.TryResolve<ISceneLoadManager>(out var sceneLoaderManager);
            await sceneLoaderManager.LoadSceneAsync(MenuScene);
        }

        private async UniTask InitAllManagers()
        {
            var locator = Locator.Instance;

            var timeManager = new TimeManager();
            var sceneLoadManager = new SceneLoadManager();
            var saveManager = new SaveManager();
            var poolManager = new PoolManager();
            
            locator.Register<ITimeManager>(timeManager);
            locator.Register<ISceneLoadManager>(sceneLoadManager);
            locator.Register<ISaveManager>(saveManager);
            locator.Register<IPoolManager>(poolManager);
            
            timeManager.Initialize();
            sceneLoadManager.Initialize();
            saveManager.Initialize();
            poolManager.Initialize();
            
            await timeManager.AsyncInitialize();
            await sceneLoadManager.AsyncInitialize();
            await saveManager.AsyncInitialize();
            await poolManager.AsyncInitialize();
        }

        private async UniTask InitLifeManager()
        {
            var locator = Locator.Instance;
            var lifeManager = new LifeManager();
            locator.Register<ILifeManager>(lifeManager);
            
            lifeManager.Initialize();
            await lifeManager.AsyncInitialize();
        }
    }
}
