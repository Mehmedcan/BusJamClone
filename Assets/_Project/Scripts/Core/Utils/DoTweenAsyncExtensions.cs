using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace _Project.Scripts.Core.Utils
{
    public static class DoTweenAsyncExtensions
    {
        public static UniTask ToUniTask(this Tween tween, TweenCancelBehaviour tweenCancelBehaviour = TweenCancelBehaviour.Kill, CancellationToken cancellationToken = default)
        {
            if (!tween.IsActive()) return UniTask.CompletedTask;
            return new UniTask(TweenConfiguredSource.Create(tween, tweenCancelBehaviour, cancellationToken, CallbackType.Kill, out var token), token);
        }
    }

    sealed class TweenConfiguredSource : IUniTaskSource, ITaskPoolNode<TweenConfiguredSource>
    {
        static TaskPool<TweenConfiguredSource> pool;
        TweenConfiguredSource nextNode;
        public ref TweenConfiguredSource NextNode => ref nextNode;

        static TweenConfiguredSource()
        {
            TaskPool.RegisterSizeGetter(typeof(TweenConfiguredSource), () => pool.Size);
        }

        readonly TweenCallback onCompleteCallbackDelegate;

        Tween tween;
        TweenCancelBehaviour cancelBehaviour;
        CancellationToken cancellationToken;
        CancellationTokenRegistration cancellationRegistration;
        CallbackType callbackType;
        bool canceled;

        TweenCallback originalCompleteAction;
        UniTaskCompletionSourceCore<AsyncUnit> core;

        TweenConfiguredSource()
        {
            onCompleteCallbackDelegate = OnCompleteCallbackDelegate;
        }

        public static IUniTaskSource Create(Tween tween, TweenCancelBehaviour cancelBehaviour, CancellationToken cancellationToken, CallbackType callbackType, out short token)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                DoCancelBeforeCreate(tween, cancelBehaviour);
                return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
            }

            if (!pool.TryPop(out var result))
            {
                result = new TweenConfiguredSource();
            }

            result.tween = tween;
            result.cancelBehaviour = cancelBehaviour;
            result.cancellationToken = cancellationToken;
            result.callbackType = callbackType;
            result.canceled = false;

            switch (callbackType)
            {
                case CallbackType.Kill:
                    result.originalCompleteAction = tween.onKill;
                    tween.onKill = result.onCompleteCallbackDelegate;
                    break;
                case CallbackType.Complete:
                    result.originalCompleteAction = tween.onComplete;
                    tween.onComplete = result.onCompleteCallbackDelegate;
                    break;
                case CallbackType.Pause:
                    result.originalCompleteAction = tween.onPause;
                    tween.onPause = result.onCompleteCallbackDelegate;
                    break;
                case CallbackType.Play:
                    result.originalCompleteAction = tween.onPlay;
                    tween.onPlay = result.onCompleteCallbackDelegate;
                    break;
                case CallbackType.Rewind:
                    result.originalCompleteAction = tween.onRewind;
                    tween.onRewind = result.onCompleteCallbackDelegate;
                    break;
                case CallbackType.StepComplete:
                    result.originalCompleteAction = tween.onStepComplete;
                    tween.onStepComplete = result.onCompleteCallbackDelegate;
                    break;
                default:
                    break;
            }

            if (result.originalCompleteAction == result.onCompleteCallbackDelegate)
            {
                result.originalCompleteAction = null;
            }

            if (cancellationToken.CanBeCanceled)
            {
                result.cancellationRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(x =>
                {
                    var source = (TweenConfiguredSource)x;
                    switch (source.cancelBehaviour)
                    {
                        case TweenCancelBehaviour.Kill:
                        default:
                            source.tween.Kill(false);
                            break;
                        case TweenCancelBehaviour.KillAndCancelAwait:
                            source.canceled = true;
                            source.tween.Kill(false);
                            break;
                        case TweenCancelBehaviour.KillWithCompleteCallback:
                            source.tween.Kill(true);
                            break;
                        case TweenCancelBehaviour.KillWithCompleteCallbackAndCancelAwait:
                            source.canceled = true;
                            source.tween.Kill(true);
                            break;
                        case TweenCancelBehaviour.Complete:
                            source.tween.Complete(false);
                            break;
                        case TweenCancelBehaviour.CompleteAndCancelAwait:
                            source.canceled = true;
                            source.tween.Complete(false);
                            break;
                        case TweenCancelBehaviour.CompleteWithSequenceCallback:
                            source.tween.Complete(true);
                            break;
                        case TweenCancelBehaviour.CompleteWithSequenceCallbackAndCancelAwait:
                            source.canceled = true;
                            source.tween.Complete(true);
                            break;
                        case TweenCancelBehaviour.CancelAwait:
                            source.RestoreOriginalCallback();
                            source.core.TrySetCanceled(source.cancellationToken);
                            break;
                    }
                }, result);
            }

            TaskTracker.TrackActiveTask(result, 3);

            token = result.core.Version;
            return result;
        }

        void OnCompleteCallbackDelegate()
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (this.cancelBehaviour == TweenCancelBehaviour.KillAndCancelAwait
                    || this.cancelBehaviour == TweenCancelBehaviour.KillWithCompleteCallbackAndCancelAwait
                    || this.cancelBehaviour == TweenCancelBehaviour.CompleteAndCancelAwait
                    || this.cancelBehaviour == TweenCancelBehaviour.CompleteWithSequenceCallbackAndCancelAwait
                    || this.cancelBehaviour == TweenCancelBehaviour.CancelAwait)
                {
                    canceled = true;
                }
            }

            if (canceled)
            {
                core.TrySetCanceled(cancellationToken);
            }
            else
            {
                originalCompleteAction?.Invoke();
                core.TrySetResult(AsyncUnit.Default);
            }
        }

        static void DoCancelBeforeCreate(Tween tween, TweenCancelBehaviour tweenCancelBehaviour)
        {
            switch (tweenCancelBehaviour)
            {
                case TweenCancelBehaviour.Kill:
                default:
                    tween.Kill(false);
                    break;
                case TweenCancelBehaviour.KillAndCancelAwait:
                    tween.Kill(false);
                    break;
                case TweenCancelBehaviour.KillWithCompleteCallback:
                    tween.Kill(true);
                    break;
                case TweenCancelBehaviour.KillWithCompleteCallbackAndCancelAwait:
                    tween.Kill(true);
                    break;
                case TweenCancelBehaviour.Complete:
                    tween.Complete(false);
                    break;
                case TweenCancelBehaviour.CompleteAndCancelAwait:
                    tween.Complete(false);
                    break;
                case TweenCancelBehaviour.CompleteWithSequenceCallback:
                    tween.Complete(true);
                    break;
                case TweenCancelBehaviour.CompleteWithSequenceCallbackAndCancelAwait:
                    tween.Complete(true);
                    break;
                case TweenCancelBehaviour.CancelAwait:
                    break;
            }
        }

        public void GetResult(short token)
        {
            try
            {
                core.GetResult(token);
            }
            finally
            {
                TryReturn();
            }
        }

        public UniTaskStatus GetStatus(short token)
        {
            return core.GetStatus(token);
        }

        public UniTaskStatus UnsafeGetStatus()
        {
            return core.UnsafeGetStatus();
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            core.OnCompleted(continuation, state, token);
        }

        bool TryReturn()
        {
            TaskTracker.RemoveTracking(this);
            core.Reset();
            cancellationRegistration.Dispose();

            RestoreOriginalCallback();

            tween = default;
            cancellationToken = default;
            originalCompleteAction = default;
            return pool.TryPush(this);
        }

        void RestoreOriginalCallback()
        {
            switch (callbackType)
            {
                case CallbackType.Kill:
                    tween.onKill = originalCompleteAction;
                    break;
                case CallbackType.Complete:
                    tween.onComplete = originalCompleteAction;
                    break;
                case CallbackType.Pause:
                    tween.onPause = originalCompleteAction;
                    break;
                case CallbackType.Play:
                    tween.onPlay = originalCompleteAction;
                    break;
                case CallbackType.Rewind:
                    tween.onRewind = originalCompleteAction;
                    break;
                case CallbackType.StepComplete:
                    tween.onStepComplete = originalCompleteAction;
                    break;
                default:
                    break;
            }
        }
    }

    public enum CallbackType
    {
        Kill,
        Complete,
        Pause,
        Play,
        Rewind,
        StepComplete
    }

    public enum TweenCancelBehaviour
    {
        Kill,
        KillWithCompleteCallback,
        Complete,
        CompleteWithSequenceCallback,
        CancelAwait,

        // AndCancelAwait
        KillAndCancelAwait,
        KillWithCompleteCallbackAndCancelAwait,
        CompleteAndCancelAwait,
        CompleteWithSequenceCallbackAndCancelAwait
    }
}