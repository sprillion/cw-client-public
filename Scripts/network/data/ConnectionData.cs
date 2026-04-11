using System.Collections.Generic;
using UnityEngine;

namespace network
{
    [CreateAssetMenu(fileName = "ConnectionData", menuName = "Data/Connection")]
    public class ConnectionData : ScriptableObject
    {
        public bool ConnectToLocalServer;
        public AccountType AccountType;

        private readonly Dictionary<AccountType, string> _tokensMap = new Dictionary<AccountType, string>()
        {
            {AccountType.Account1, "de6e5fe8-f87c-40be-ab93-3b0c6ce55fa7"}, 
            {AccountType.Account2, "dc12be71-42f2-4d7c-abdc-3c7827d28ad2"}, 
            {AccountType.Account3, "7f73c3f3-1ed1-4aba-b15a-6b22bcc9edce"}, 
        };

        public string GetToken()
        {
            return _tokensMap.GetValueOrDefault(AccountType);
        }
    }

    public enum AccountType
    {
        SavedAccaunt,
        Account1,
        Account2,
        Account3,
    }
}