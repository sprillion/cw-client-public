using Cysharp.Threading.Tasks;

namespace infrastructure.services.platform.core.auth
{
    public interface IPlatformAuthProvider
    {
        bool IsAuthenticated { get; }
        UniTask<string> Authenticate();
    }
}
