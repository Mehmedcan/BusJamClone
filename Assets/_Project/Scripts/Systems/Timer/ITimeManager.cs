using System;
using _Project.Scripts.Systems.Base;

namespace _Project.Scripts.Systems.Timer
{
    public interface ITimeManager : IBaseManager
    {
        public IDisposable StartTimer(int seconds, Action<int> onSecond = null, Action onCompleted = null);
    }
}
