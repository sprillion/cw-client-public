using System;

namespace infrastructure.services.loading
{
    public interface ILoadingService
    {
        bool IsLoading { get; }
        event Action Loaded;
        event Action LoadingMapException;
    }
}