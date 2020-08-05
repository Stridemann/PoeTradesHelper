using System;

namespace PoeTradesHelper.Chat
{
    public class ChatMessage
    {
        public string Nick { get; }
        public MessageType MessageType { get; }
        public string Message { get; }

        public ChatMessage(string nick, MessageType messageType, string message)
        {
            Nick = nick;
            MessageType = messageType;
            Message = message;
        }
    }
}