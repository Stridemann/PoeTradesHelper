using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PoeTradesHelper.Chat
{
    public class MessagesController
    {
        private readonly Regex _directMessageRegex;
        private readonly Regex _notOnlineRegex;
        private readonly Regex _leftAreaRegex;
        private readonly Regex _joinedAreaRegex;
        public event Action<ChatMessage> ChatMessageReceived = delegate { };

        public MessagesController()
        {
            _directMessageRegex =
                new Regex(@"^\@(?'MsgDir'From|To)\s(?'Nick'\S+): (?'Message'.*)",
                    RegexOptions.Compiled);
            _notOnlineRegex = new Regex(@"^(?'Nick'.*) is not online\.$", RegexOptions.Compiled);
            _joinedAreaRegex = new Regex(@"^(?'Nick'.*) has joined the area\.$", RegexOptions.Compiled);
            _leftAreaRegex = new Regex(@"^(?'Nick'.*) has left the area\.$", RegexOptions.Compiled);
        }

        public void ReceiveMessage(string messageText)
        {
            var finalMessage = Regex.Replace(messageText, @"\[\d\d\:\d\d\]\s", string.Empty); //remove time
            finalMessage = Regex.Replace(finalMessage, @"<.+>\s", string.Empty); //remove nick tag

            var match = _directMessageRegex.Match(finalMessage);
            if (match.Success)
            {
                var messageType = match.Groups["MsgDir"].Value == "To" ? MessageType.To : MessageType.From;
                var target = match.Groups["Nick"].Value;
                var messageValue = match.Groups["Message"].Value;

                var chatMessage = new ChatMessage(target, messageType, messageValue);
                ChatMessageReceived(chatMessage);
                return;
            }

            match = _notOnlineRegex.Match(finalMessage);
            if (match.Success)
            {
                var target = match.Groups["Nick"].Value;
                var chatMessage = new ChatMessage(target, MessageType.NotOnline, string.Empty);
                ChatMessageReceived(chatMessage);
                return;
            }

            match = _joinedAreaRegex.Match(finalMessage);
            if (match.Success)
            {
                var target = match.Groups["Nick"].Value;
                var chatMessage = new ChatMessage(target, MessageType.JoinArea, string.Empty);
                ChatMessageReceived(chatMessage);
                return;
            }

            match = _leftAreaRegex.Match(finalMessage);
            if (match.Success)
            {
                var target = match.Groups["Nick"].Value;
                var chatMessage = new ChatMessage(target, MessageType.LeftArea, string.Empty);
                ChatMessageReceived(chatMessage);
                return;
            }

            if (finalMessage == "Trade accepted.")
            {
                ChatMessageReceived(new ChatMessage(string.Empty, MessageType.TradeAccepted, string.Empty));
                return;
            }

            if (finalMessage == "Trade cancelled.")
            {
                ChatMessageReceived(new ChatMessage(string.Empty, MessageType.TradeCancelled, string.Empty));
                return;
            }
        }
    }

    public enum MessageType
    {
        From,
        To,
        JoinArea,
        LeftArea,
        NotOnline,
        TradeAccepted,
        TradeCancelled,
    }
}