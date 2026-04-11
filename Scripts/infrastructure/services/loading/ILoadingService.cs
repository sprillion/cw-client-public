using System;

namespace infrastructure.services.loading
{
    public enum LoadingStage
    {
        Connecting,
        CheckingVersion,
        LoadingMap,
        LoadingNpc,
        Authorizing,
        LoadingPlayerData,
        CreatingWorld,
        Finalizing,
        ServerUnavailable,
        VersionMismatch,
        Reconnecting,
    }

    public interface ILoadingService
    {
        bool IsLoading { get; }
        LoadingStage CurrentStage { get; }
        event Action Loaded;
        event Action LoadingMapException;
        event Action<LoadingStage> OnStageChanged;
        event Action OnServerUnavailable;
        event Action OnVersionMismatch;
        void Retry();
    }
}
