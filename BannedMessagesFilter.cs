namespace PoeTradesHelper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Chat;

    public class BannedMessagesFilter
    {
        private readonly Settings _settings;
        private readonly List<BannedMessage> _bannedMessages = new List<BannedMessage>();

        public BannedMessagesFilter(Settings settings)
        {
            _settings = settings;
        }

        public event Action<ChatMessage> MessagePassed = delegate { };

        public void FilterMessage(ChatMessage chatMessage)
        {
            _bannedMessages.RemoveAll(x => x.IsOutOfTime(_settings.BanMessageTimeMinutes.Value));

            foreach (var bannedMessage in _bannedMessages)
            {
                if (bannedMessage.Message == chatMessage.Message)
                {
                    return;
                }
            }

            MessagePassed(chatMessage);
        }

        public void BanMessage(string message)
        {
            if (_bannedMessages.Any(x => x.Message == message))
                return;
            _bannedMessages.Add(new BannedMessage(message));
        }

        private class BannedMessage
        {
            public readonly Stopwatch _timestamp;
            public string Message { get; }

            public BannedMessage(string message)
            {
                Message = message;
                _timestamp = Stopwatch.StartNew();
            }

            public bool IsOutOfTime(double thresholdMin)
            {
                return _timestamp.Elapsed.TotalMinutes > thresholdMin;
            }
        }
    }
}
