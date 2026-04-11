using System;
using secrets;
using tools;
using UnityEngine;

namespace network
{
    public class NetworkManager : MonoBehaviour, INetworkManager
    {
        public enum FromClientMessage : byte
        {
            GetServerTime,
            CheckVersion,
        }

        public enum FromServerMessage : byte
        {
            ServerTime,
            VersionCheckResult,
        }
        
        private WebSocketManager _gameWebSocketManager;

        public bool ConnectedToServer { get; private set; }
        
        public event Action Update;
        public event Action<Message> OnMessageEvent;
        public event Action OnServerConnected;
        public event Action OnServerDisconnected;
        public event Action<bool> OnVersionCheckResult;

        public static int CurrentTick;
        public static DateTime ServerNow => DateTime.UtcNow + _serverTimeOffset;

        private static DateTime _serverTime = DateTime.UtcNow;
        private static TimeSpan _serverTimeOffset = TimeSpan.Zero;

        public static void UpdateTick(int tick)
        {
            if (tick <= CurrentTick) return;
            CurrentTick = tick;
        }
        
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
        
        private void OnApplicationFocus(bool focus)
        {
            if (!ConnectedToServer) return;

            if (focus)
                RequestServerTime();
        }

        public void SendMessage(Message message)
        {
            _gameWebSocketManager.SendMessage(message);
        }

        public void Connect()
        {
            if (_gameWebSocketManager != null)
            {
                _gameWebSocketManager.OnOpenEvent -= OnWebSockedConnected;
                _gameWebSocketManager.OnMessageEvent -= OnMessage;
                _gameWebSocketManager.OnCloseEvent -= OnWebSocketDisconnected;
                _gameWebSocketManager.OnErrorEvent -= OnWebSocketError;
            }

            var connectToLocalServer = false;
            if (Application.isEditor)
            {
                connectToLocalServer = GameResources.Data.connection_data<ConnectionData>().ConnectToLocalServer;
            }

            var domen = connectToLocalServer ? $"localhost:{SecretKey.Port}" : SecretKey.Domen;
            _gameWebSocketManager = new WebSocketManager($"wss://{domen}/ws/");

            _gameWebSocketManager.OnOpenEvent += OnWebSockedConnected;
            _gameWebSocketManager.OnMessageEvent += OnMessage;
            _gameWebSocketManager.OnCloseEvent += OnWebSocketDisconnected;
            _gameWebSocketManager.OnErrorEvent += OnWebSocketError;
        }
        
        public void ReceiveMessage(Message message)
        {
            var type = (FromServerMessage)message.GetByte();

            switch (type)
            {
                case FromServerMessage.ServerTime:
                    SetServerTime(message);
                    break;
                case FromServerMessage.VersionCheckResult:
                    OnVersionCheckResult?.Invoke(message.GetBool());
                    break;
            }
        }
        
        private void SetServerTime(Message message)
        {
            _serverTime = DateTimeOffset.FromUnixTimeMilliseconds(message.GetLong()).UtcDateTime;
            _serverTimeOffset = _serverTime - DateTime.UtcNow;
        }
        
        private void RequestServerTime()
        {
            var message = new Message(MessageType.Network)
                .AddByte(FromClientMessage.GetServerTime.ToByte());
            SendMessage(message);
        }

        private void OnMessage(Message message)
        {
            OnMessageEvent?.Invoke(message);
        }

        private void OnWebSockedConnected()
        {
            ConnectedToServer = true;
            OnServerConnected?.Invoke();
        }

        private void OnWebSocketDisconnected()
        {
            ConnectedToServer = false;
            Update = null;
            OnServerDisconnected?.Invoke();
        }

        private void OnWebSocketError()
        {
            if (!ConnectedToServer)
            {
                Update = null;
                OnServerDisconnected?.Invoke();
            }
        }

    }
}