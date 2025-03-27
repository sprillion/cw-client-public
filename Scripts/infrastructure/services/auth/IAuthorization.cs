using network;

namespace infrastructure.services.auth
{
    public interface IAuthorization : IReceiver
    {
        void Authorize();
    }
}