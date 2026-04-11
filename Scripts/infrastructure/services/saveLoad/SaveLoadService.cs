using System.Linq;
using Sirenix.Utilities;

using UnityEngine;

namespace infrastructure.services.saveLoad
{
    public class SaveLoadService : ISaveLoadService
    {
        private const string TokenKey = "Token";

        public bool HasToken()
        {
            var key = TokenKey;
            
            if (!Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().IsNullOrEmpty())
            {
                key = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().First() + TokenKey;
            }
            return PlayerPrefs.HasKey(key);
        }
        
        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("[SaveLoad] SetToken: token is null or empty, skipping");
                return;
            }
            var key = TokenKey;
            if (!Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().IsNullOrEmpty())
            {
                key = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().First() + TokenKey;
            }
            PlayerPrefs.SetString(key, token);
        }

        public string GetToken()
        {
            var key = TokenKey;
            if (!Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().IsNullOrEmpty())
            {
                key = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().First() + TokenKey;
            }
            return PlayerPrefs.GetString(key);
        }

        public void SetJson(string key, string json)
        {
            PlayerPrefs.SetString(GetPrefixedKey(key), json);
        }

        public string GetJson(string key)
        {
            return PlayerPrefs.GetString(GetPrefixedKey(key));
        }

        public bool HasJson(string key)
        {
            return PlayerPrefs.HasKey(GetPrefixedKey(key));
        }

        private string GetPrefixedKey(string key)
        {
            if (!Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().IsNullOrEmpty())
            {
                return Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().First() + key;
            }
            return key;
        }
    }
}