using System.Collections.Generic;
using PoeTradesHelper.Chat;

namespace PoeTradesHelper
{
    public class AreaPlayersController
    {
        private readonly HashSet<string> _playersInArea = new HashSet<string>();

        public void OnChatMessageReceived(ChatMessage message)
        {
            if (message.MessageType == MessageType.LeftArea)
            {
                _playersInArea.Remove(message.Nick);
            }
            else if (message.MessageType == MessageType.JoinArea)
            {
                if (!_playersInArea.Contains(message.Nick))
                    _playersInArea.Add(message.Nick);
            }
        }

        public bool IsPlayerInArea(string nick)
        {
            return _playersInArea.Contains(nick);
        }

        public void Clear()
        {
            _playersInArea.Clear();
        }
    }
}