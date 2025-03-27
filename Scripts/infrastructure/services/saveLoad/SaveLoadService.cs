using System.Linq;
using Sirenix.Utilities;
using Unity.Multiplayer.Playmode;
using UnityEngine;

namespace infrastructure.services.saveLoad
{
    public class SaveLoadService : ISaveLoadService
    {
        private const string TokenKey = "Token";

        public bool HasToken()
        {
            var key = TokenKey;
            if (!CurrentPlayer.ReadOnlyTags().IsNullOrEmpty())
            {
                key = CurrentPlayer.ReadOnlyTags().First() + TokenKey;
            }
            return PlayerPrefs.HasKey(key);
        }
        
        public void SetToken(string token)
        {
            var key = TokenKey;
            if (!CurrentPlayer.ReadOnlyTags().IsNullOrEmpty())
            {
                key = CurrentPlayer.ReadOnlyTags().First() + TokenKey;
            }
            PlayerPrefs.SetString(key, token);
        }

        public string GetToken()
        {
            var key = TokenKey;
            if (!CurrentPlayer.ReadOnlyTags().IsNullOrEmpty())
            {
                key = CurrentPlayer.ReadOnlyTags().First() + TokenKey;
            }
            return PlayerPrefs.GetString(key);
        }
    }
}