using System;

namespace infrastructure.services.gameLoop
{
    public interface IGameLoop
    {
        void AddResponseAction(Action action);
    }
}