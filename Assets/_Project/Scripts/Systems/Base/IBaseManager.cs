
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Systems.Base
{
    public interface IBaseManager
    {
        string LogTag { get; }
        UniTask AsyncInitialize();

        void Initialize();
        
        void Dispose();
    }
}
