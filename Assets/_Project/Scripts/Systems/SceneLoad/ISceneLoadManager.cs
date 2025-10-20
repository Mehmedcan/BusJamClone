using _Project.Scripts.Systems.Base;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Systems.SceneLoad
{
    public interface ISceneLoadManager : IBaseManager
    {
        public UniTask LoadSceneAsync(string sceneName);
    }
}