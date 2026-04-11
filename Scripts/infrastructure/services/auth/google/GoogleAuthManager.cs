using System;
using UnityEngine;

public class GoogleAuthManager : MonoBehaviour
{
    private static void Log(string message)
    {
        Debug.Log(message);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            System.IO.File.AppendAllText(
                Application.persistentDataPath + "/googleauth.log",
                $"{System.DateTime.Now:HH:mm:ss} {message}\n"
            );
        }
        catch { }
#endif
    }

    public static GoogleAuthManager Instance { get; private set; }

    public event Action<string> OnAuthSuccess; // idToken
    public event Action<string> OnAuthFailed;
    public event Action OnAuthCanceled;

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
        Debug.Log("[GoogleAuth] Editor mode — симулируем успех");
        OnGoogleAuthSuccess("fake_google_id_token");

#elif UNITY_ANDROID
        Log("[GoogleAuth] Login() called on Android");
        try
        {
            using var pluginClass = new AndroidJavaClass("com.yourgame.googleauth.GoogleAuthPlugin");
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");
            Log("[GoogleAuth] Plugin and activity resolved, calling login...");
            pluginClass.CallStatic("login", activity);
            Log("[GoogleAuth] login() called successfully");
        }
        catch (Exception e)
        {
            Log($"[GoogleAuth] Exception in Login: {e}");
        }
#endif
    }

    // Вызывается из нативного кода через UnitySendMessage
    public void OnGoogleAuthSuccess(string idToken)
    {
        Log($"[GoogleAuth] Success! Token length: {idToken?.Length}");
        OnAuthSuccess?.Invoke(idToken);
    }

    // Вызывается из нативного кода через UnitySendMessage
    public void OnGoogleAuthFail(string error)
    {
        Log($"[GoogleAuth] Failed: {error}");

        if (error == "canceled")
            OnAuthCanceled?.Invoke();
        else
            OnAuthFailed?.Invoke(error);
    }
}
