namespace network
{
    public interface IReceiver
    {
        void ReceiveMessage(Message message);
    }
}