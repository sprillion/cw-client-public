using System;
using UnityEngine;
using HybridWebSocket;
using network;

public class WebSocketManager
{
    private readonly WebSocket _webSocket;

    public event Action<Message> OnMessageEvent;
    public event Action OnOpenEvent;
    public event Action OnCloseEvent;
    public event Action OnErrorEvent;

    public WebSocketManager(string url)
    {
        _webSocket = WebSocketFactory.CreateInstance(url);

        _webSocket.OnOpen += OnOpen;
        _webSocket.OnMessage += OnMessage;
        _webSocket.OnError += OnError;
        _webSocket.OnClose += OnClose;
        
        _webSocket.Connect();
    }


    public void SendMessage(Message message)
    {
        _webSocket.Send(message.ToArray());
        message.Dispose();
    }

    public void Close()
    {
        _webSocket.Close();
    }

    private void OnOpen()
    {
        Debug.Log($"WS connected! State: {_webSocket.GetState()}");

        OnOpenEvent?.Invoke();
    }

    private void OnClose(WebSocketCloseCode code)
    {
        Debug.Log("WS closed with code: " + code.ToString());

        OnCloseEvent?.Invoke();
    }

    private void OnError(string errorMessage)
    {
        Debug.Log("WS error: " + errorMessage);

        OnErrorEvent?.Invoke();
    }

    private void OnMessage(byte[] bytes)
    {
        OnMessageEvent?.Invoke(new Message(bytes));
    }
}