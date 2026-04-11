namespace infrastructure.services.chat
{
    public class ChatMessage
    {
        public int PlayerId { get; set; }
        public string Nickname { get; set; }
        public string Clan { get; set; }
        public string Message { get; set; }
        
        public ChatMessageType ChatMessageType { get; set; }
    }
}