using System;
using _Project.Data.GameData;
using _Project.Scripts.Systems.Base;
using UniRx;

namespace _Project.Scripts.Systems.Life
{
    public interface IUserDataManager : IBaseManager
    {
        public ReactiveProperty<int> RemainingTimeForNextLife { get; }
        public event EventHandler<UserConfig> OnUserLifeChanged;
        
        public (UserConfig, int/*earned gold*/) WinLevel();
        public UserConfig LoseLevel();
        public void SetupLifeDataIfNeeded();
    }
}
