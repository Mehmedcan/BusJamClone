using System;
using Cysharp.Threading.Tasks;
using UniRx;

namespace _Project.Scripts.Systems.Timer
{
    public class TimeManager : ITimeManager
    {
        public string LogTag => "[TimeManager]";

        private IObservable<long> _tickShared;
        public IObservable<long> Tick => _tickShared;
        private readonly CompositeDisposable _disposables = new();

        #region Base

        public UniTask AsyncInitialize()
        {
            return UniTask.CompletedTask;
        }

        public void Initialize()
        {
            _tickShared = Observable.Interval(TimeSpan.FromSeconds(1)).Share();
        }

        public void Dispose()
        {
            _tickShared = null;
            _disposables?.Dispose();
        }

        #endregion
        
        
        public IDisposable StartTimer(int seconds, Action<int> onSecond = null, Action onCompleted = null)
        {
            if (seconds <= 0)
            {
                onCompleted?.Invoke();
                return Disposable.Empty;
            }

            var sub = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                .Select(elapsed => seconds - (int)elapsed)
                .TakeWhile(remaining => remaining > 0)
                .Do(remaining => onSecond?.Invoke(remaining))
                .DoOnCompleted(() => onSecond?.Invoke(0))
                .DoOnCompleted(() => onCompleted?.Invoke())  
                .Subscribe();

            _disposables.Add(sub);
            return sub;
        }
    }
}
