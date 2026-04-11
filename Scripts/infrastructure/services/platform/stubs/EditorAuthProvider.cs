using Cysharp.Threading.Tasks;
using infrastructure.services.platform.core.auth;
using UnityEngine;

namespace infrastructure.services.platform.stubs
{
    public class EditorAuthProvider : IPlatformAuthProvider
    {
        public bool IsAuthenticated { get; private set; }

        public async UniTask<string> Authenticate()
        {
            Debug.Log("[EditorAuth] Authenticate() called — returning null (guest)");
            IsAuthenticated = true;
            await UniTask.CompletedTask;
            return null;
        }
    }
}
