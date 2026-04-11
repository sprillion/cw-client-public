using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using infrastructure.services.auth;
using infrastructure.services.chat;
using infrastructure.services.chests;
using infrastructure.services.clan;
using infrastructure.services.craft;
using infrastructure.services.house;
using infrastructure.services.inventory;
using infrastructure.services.lumber;
using infrastructure.services.mine;
using infrastructure.services.mobs;
using infrastructure.services.npc;
using infrastructure.services.platform.core.payment;
using infrastructure.services.players;
using infrastructure.services.quests;
using infrastructure.services.shop;
using infrastructure.services.navigation;
using infrastructure.services.transport;
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
        private readonly IQuestsService _questsService;
        private readonly IChatService _chatService;
        private readonly ICraftService _craftService;
        private readonly IShopService _shopService;
        private readonly IMineService _mineService;
        private readonly ILumberService _lumberService;
        private readonly IHouseService _houseService;
        private readonly IClanService _clanService;
        private readonly IPaymentService _paymentService;
        private readonly ITransportService _transportService;
        private readonly INavigationService _navigationService;

        public GameLoop(
            INetworkManager networkManager,
            IAuthorization authorization,
            ICharacterService characterService,
            IMobService mobService,
            IInventoryService inventoryService,
            IChestsService chestsService,
            INpcService npcService,
            IQuestsService questsService,
            IChatService chatService,
            ICraftService craftService,
            IShopService shopService,
            IMineService mineService,
            ILumberService lumberService,
            IHouseService houseService,
            IClanService clanService,
            IPaymentService paymentService,
            ITransportService transportService,
            INavigationService navigationService
        )
        {
            _networkManager = networkManager;
            _authorization = authorization;
            _characterService = characterService;
            _mobService = mobService;
            _inventoryService = inventoryService;
            _chestsService = chestsService;
            _npcService = npcService;
            _questsService = questsService;
            _chatService = chatService;
            _craftService = craftService;
            _shopService = shopService;
            _mineService = mineService;
            _lumberService = lumberService;
            _houseService = houseService;
            _clanService = clanService;
            _paymentService = paymentService;
            _transportService = transportService;
            _navigationService = navigationService;

            _networkManager.Update += ReadMessages;
            _networkManager.Update += ReadActions;
            _networkManager.OnMessageEvent += AddToUnityThread;
        }

        public void AddToUnityThread(Action action)
        {
            _responseActions.Enqueue(action);
        }

        private void AddToUnityThread(Message message)
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
            IReceiver receiver = message.MessageType switch
            {
                MessageType.Network => _networkManager,
                MessageType.Auth => _authorization,
                MessageType.Character => _characterService,
                MessageType.Mob => _mobService,
                MessageType.Inventory => _inventoryService,
                MessageType.Chest => _chestsService,
                MessageType.Npc => _npcService,
                MessageType.Quests => _questsService,
                MessageType.Chat => _chatService,
                MessageType.Crafts => _craftService,
                MessageType.Shop => _shopService,
                MessageType.Mine => _mineService,
                MessageType.Lumber => _lumberService,
                MessageType.House => _houseService,
                MessageType.Clan => _clanService,
                MessageType.Payment => _paymentService,
                MessageType.Transport  => _transportService,
                MessageType.Navigation => _navigationService,
                _ => null
            };

            receiver?.ReceiveMessage(message);
            
            message.Dispose();
        }
    }
}