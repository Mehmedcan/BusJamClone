using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Systems.SceneLoad
{
    public class SceneLoadManager : ISceneLoadManager
    {
        public string LogTag => "[SceneManager]";
        
        private readonly ReactiveProperty<float> ProgressStream = new(0f);
        private readonly Subject<string> SceneLoaded = new();

        #region Base

        public UniTask AsyncInitialize()
        {
            return UniTask.CompletedTask;
        }

        public void Initialize()
        {
            // Initialization logic if needed
        }

        public void Dispose()
        {
            ProgressStream?.Dispose();
            SceneLoaded?.Dispose();
        }

        #endregion
        
        
        public async UniTask LoadSceneAsync(string sceneName)
        {
            var target = SceneManager.GetSceneByName(sceneName);
            if (target.isLoaded)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            var newSceneLoader = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await newSceneLoader.ToUniTask();

            var loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
                SceneManager.SetActiveScene(loadedScene);

            if (activeScene.IsValid() && activeScene.isLoaded)
                await SceneManager.UnloadSceneAsync(activeScene).ToUniTask();
        }
        
    }
}
