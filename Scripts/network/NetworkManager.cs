using System;
using secrets;
using UnityEngine;

namespace network
{
    public class NetworkManager : MonoBehaviour, INetworkManager
    {
        private const ushort TickDivergenceTolerance = 1;
        
        [SerializeField] private bool _connectToLocalServer;

        private WebSocketManager _gameWebSocketManager;

        public bool ConnectedToServer { get; private set; }
        
        public event Action Update;
        public event Action<Message> OnMessageEvent;
        public event Action OnServerConnected;
       
        private void FixedUpdate()
        {
            Update?.Invoke();
        }

        private void OnDestroy()
        {
            Update = null;
            try
            {
                _gameWebSocketManager.Close();
            }
            catch
            {
                // ignored
            }
        }

        public void SendMessage(Message message)
        {
            _gameWebSocketManager.SendMessage(message);
        }

        public void Connect()
        {
            if (!Application.isEditor)
            {
                _connectToLocalServer = false;
            }
            
            var domen = _connectToLocalServer ? $"localhost:{SecretKey.Port}" : SecretKey.Domen;
            _gameWebSocketManager = new WebSocketManager($"wss://{domen}/ws/");

            _gameWebSocketManager.OnOpenEvent += OnWebSockedConnected;
            _gameWebSocketManager.OnMessageEvent += OnMessage;
        }

        private void OnMessage(Message message)
        {
            OnMessageEvent?.Invoke(message);
        }

        private void OnWebSockedConnected()
        {
            OnServerConnected?.Invoke();
            ConnectedToServer = true;
        }
        
    }
}