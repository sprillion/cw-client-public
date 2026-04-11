using System;

namespace infrastructure.services.gameLoop
{
    public interface IGameLoop
    {
        void AddToUnityThread(Action action);
    }
}