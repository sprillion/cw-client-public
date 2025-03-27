using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using infrastructure.services.auth;
using infrastructure.services.chests;
using infrastructure.services.inventory;
using infrastructure.services.mobs;
using infrastructure.services.npc;
using infrastructure.services.players;
using network;

namespace infrastructure.services.gameLoop
{
    public class GameLoop : IGameLoop
    {
        private readonly ConcurrentQueue<Message> _responseMessages = new ConcurrentQueue<Message>();
        private readonly Queue<Action> _responseActions = new Queue<Action>();

        private readonly INetworkManager _networkManager;
        private readonly IAuthorization _authorization;
        private readonly ICharacterService _characterService;
        private readonly IMobService _mobService;
        private readonly IInventoryService _inventoryService;
        private readonly IChestsService _chestsService;
        private readonly INpcService _npcService;

        public GameLoop(
            INetworkManager networkManager,
            IAuthorization authorization,
            ICharacterService characterService,
            IMobService mobService,
            IInventoryService inventoryService,
            IChestsService chestsService,
            INpcService npcService
        )
        {
            _networkManager = networkManager;
            _authorization = authorization;
            _characterService = characterService;
            _mobService = mobService;
            _inventoryService = inventoryService;
            _chestsService = chestsService;
            _npcService = npcService;
            
            _networkManager.Update += ReadMessages;
            _networkManager.Update += ReadActions;
            _networkManager.OnMessageEvent += AddResponseMessage;
        }

        public void AddResponseAction(Action action)
        {
            _responseActions.Enqueue(action);
        }

        private void AddResponseMessage(Message message)
        {
            _responseMessages.Enqueue(message);
        }

        private void ReadMessages()
        {
            var queue = new ConcurrentQueue<Message>(_responseMessages);
            _responseMessages.Clear();
            while (queue.TryDequeue(out var response))
            {
                ReadResponse(response);
            }
        }

        private void ReadActions()
        {
            var queue = new Queue<Action>(_responseActions);
            _responseActions.Clear();
            while (queue.TryDequeue(out var response))
            {
                response?.Invoke();
            }
        }
        
        private void ReadResponse(Message message)
        {
            IReceiver receiver = message.ServerToClientId switch
            {
                ServerToClientId.Auth => _authorization,
                ServerToClientId.Character => _characterService,
                ServerToClientId.Mob => _mobService,
                ServerToClientId.Inventory => _inventoryService,
                ServerToClientId.Chest => _chestsService,
                ServerToClientId.Npc => _npcService,
                _ => null
            };

            receiver?.ReceiveMessage(message);
            
            message.Dispose();
        }
    }
}