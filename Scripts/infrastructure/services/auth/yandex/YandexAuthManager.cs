using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class YandexAuthManager : MonoBehaviour
{
    private static void Log(string message)
    {
        Debug.Log(message);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            System.IO.File.AppendAllText(
                Application.persistentDataPath + "/yandexauth.log",
                $"{System.DateTime.Now:HH:mm:ss} {message}\n"
            );
        }
        catch { }
#endif
    }

    public static YandexAuthManager Instance { get; private set; }

    public event Action<string> OnAuthSuccess; // token
    public event Action<string> OnAuthFailed;
    public event Action OnAuthCanceled;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void YandexAuthLogin();
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void Login()
    {
#if UNITY_EDITOR
        Debug.Log("[YandexAuth] Editor mode — симулируем успех");
        OnYandexAuthSuccess("editor_yandex_token");

#elif UNITY_ANDROID
        Log("[YandexAuth] Login() called on Android");
        try
        {
            using var pluginClass = new AndroidJavaClass("com.yourgame.yandexauth.YandexAuthPlugin");
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");
            Log("[YandexAuth] Plugin and activity resolved, calling login...");
            pluginClass.CallStatic("login", activity);
            Log("[YandexAuth] login() called successfully");
        }
        catch (Exception e)
        {
            Log($"[YandexAuth] Exception in Login: {e}");
        }

#elif UNITY_WEBGL
        Log("[YandexAuth] Login() called on WebGL");
        YandexAuthLogin();
#endif
    }

    // Вызывается из нативного кода через UnitySendMessage
    public void OnYandexAuthSuccess(string token)
    {
        Log($"[YandexAuth] Success! Token length: {token?.Length}");
        OnAuthSuccess?.Invoke(token);
    }

    // Вызывается из нативного кода через UnitySendMessage
    public void OnYandexAuthFail(string error)
    {
        Log($"[YandexAuth] Failed: {error}");

        if (error == "canceled")
            OnAuthCanceled?.Invoke();
        else
            OnAuthFailed?.Invoke(error);
    }
}
