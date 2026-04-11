using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class VKAuthManager : MonoBehaviour
{
    private static void Log(string message)
    {
        Debug.Log(message);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            System.IO.File.AppendAllText(
                Application.persistentDataPath + "/vkauth.log",
                $"{System.DateTime.Now:HH:mm:ss} {message}\n"
            );
        }
        catch { }
#endif
    }

    public static VKAuthManager Instance { get; private set; }
    
    public event Action<string, string> OnAuthSuccess; // userId, token
    public event Action<string> OnAuthFailed;
    public event Action OnAuthCanceled;

    // iOS нативные методы
    // #if UNITY_IOS && !UNITY_EDITOR
    //     [DllImport("__Internal")]
    //     private static extern void _VKAuth_Login();
    //     
    //     [DllImport("__Internal")]
    //     private static extern void _VKAuth_Logout();
    // #endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.parent.gameObject);
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var pluginClass = new AndroidJavaClass("com.yourgame.vkauth.VKAuthPlugin");
                var context = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity")
                    .Call<AndroidJavaObject>("getApplicationContext");
                pluginClass.CallStatic("initialize", context);
                Log("[VKAuth] VKID initialized in Awake");
            }
            catch (Exception e)
            {
                Log($"[VKAuth] Failed to initialize VKID in Awake: {e}");
            }
#endif
        }
        else Destroy(gameObject);
    }

    public void Login()
    {
#if UNITY_EDITOR
        Debug.Log("[VKAuth] Editor mode — симулируем успех");
        OnVKAuthSuccess("123456789|fake_token_for_editor");
        
#elif UNITY_ANDROID
        Log("[VKAuth] Login() called on Android");
        try
        {
            using var pluginClass = new AndroidJavaClass("com.yourgame.vkauth.VKAuthPlugin");
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");
            Log("[VKAuth] Plugin and activity resolved, calling login...");
            pluginClass.CallStatic("login", activity);
            Log("[VKAuth] login() called successfully");
        }
        catch (Exception e)
        {
            Log($"[VKAuth] Exception in Login: {e}");
        }
        
// #elif UNITY_IOS
//         _VKAuth_Login();
#endif
    }

    // Вызывается из нативного кода через UnitySendMessage
    public void OnVKAuthSuccess(string payload)
    {
        var parts = payload.Split('|');
        if (parts.Length != 2)
        {
            OnAuthFailed?.Invoke("Invalid payload format");
            return;
        }
        
        string userId = parts[0];
        string token = parts[1];
        
        Log($"[VKAuth] Success! UserID: {userId}");
        OnAuthSuccess?.Invoke(userId, token);
    }

    // Вызывается из нативного кода через UnitySendMessage
    public void OnVKAuthFail(string error)
    {
        Log($"[VKAuth] Failed: {error}");
        
        if (error == "canceled")
            OnAuthCanceled?.Invoke();
        else
            OnAuthFailed?.Invoke(error);
    }
}