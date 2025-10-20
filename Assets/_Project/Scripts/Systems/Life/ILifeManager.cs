using _Project.Scripts.Systems.Base;
using UniRx;

namespace _Project.Scripts.Systems.Life
{
    public interface ILifeManager : IBaseManager
    {
        public ReactiveProperty<int> RemainingTimeForNextLife { get; }
        public void SetupLifeDataIfNeeded();
    }
}
