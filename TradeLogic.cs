using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using PoeTradesHelper.Chat;

namespace PoeTradesHelper
{
    public class TradeLogic
    {
        private static int EntryUniqueIdCounter;
        private readonly Regex _buyRegex;
        private readonly Regex _itemPosRegex;
        private readonly Settings _settings;
        public event Action NewTradeReceived = delegate { };

        public TradeLogic(Settings settings)
        {
            _settings = settings;
            _buyRegex = new Regex(
                @"(Hi\,\sI('d like| would like) to buy your|wtb) (?'ItemName'.*) (listed for|for my) (?'CurrencyAmount'[\d.]+) (?'CurrencyType'.*) in (?'LeagueName'\w+)?(?'ExtraText'.*)");

            //\((stash tab|stash) \"(?'TabName'.*)\"\;(\sposition\:|) left (?'TabX'\d+)\, top (?'TabY'\d+)\)(?'Offer'.+|)
            _itemPosRegex = new Regex(@"\((stash tab|stash) \""(?'TabName'.*)\""\;(\sposition\:|) left (?'TabX'\d+)\, top (?'TabY'\d+)\)(?'Offer'.+|)");
        }

        public ConcurrentDictionary<int, TradeEntry> TradeEntries { get; } =
            new ConcurrentDictionary<int, TradeEntry>();

        public void OnChatMessageReceived(ChatMessage message)
        {
            if (message.MessageType == MessageType.From || message.MessageType == MessageType.To)
            {
                var match = _buyRegex.Match(message.Message);

                if (match.Success)
                    TradeMessageReceived(message, match);
            }
            else if(message.MessageType == MessageType.NotOnline)
            {
                var entryToRemove = TradeEntries.LastOrDefault(x => !x.Value.IsIncomingTrade && x.Value.PlayerNick == message.Nick).Value;
                if (entryToRemove != null)
                {
                    TradeEntries.TryRemove(entryToRemove.UniqueId, out _);
                }
            }
        }

        private void TradeMessageReceived(ChatMessage message, Match match)
        {
            if (_settings.RemoveDuplicatedTrades.Value && TradeEntries.Any(x =>
                x.Value.PlayerNick == message.Nick && x.Value.RawMessage == message.Message))
                return;

            var itemName = match.Groups["ItemName"].Value;
            var currencyType = match.Groups["CurrencyType"].Value;
            var currencyAmount = match.Groups["CurrencyAmount"].Value;
            EntryUniqueIdCounter++;

            var tradeEntry = new TradeEntry(
                itemName,
                message.Nick,
                currencyType,
                currencyAmount,
                message.MessageType == MessageType.From,
                EntryUniqueIdCounter,
                message.Message);

            //var leagueName = match.Groups["LeagueName"].Value;//TODO: Check and warn if wrong league
            var extraText = match.Groups["ExtraText"];

            if (extraText.Success)
            {
                var itemPos = _itemPosRegex.Match(extraText.Value);

                if (itemPos.Success)
                {
                    var tab = itemPos.Groups["TabName"].Value;
                    var posX = itemPos.Groups["TabX"].Value;
                    var posY = itemPos.Groups["TabY"].Value;
                    var offer = itemPos.Groups["Offer"].Value;
                    tradeEntry.ItemPosInfo = new ItemPosInfo(tab, new Vector2(int.Parse(posX), int.Parse(posY)));
                    tradeEntry.OfferText = offer;
                }
                else
                {
                    tradeEntry.OfferText = extraText.Value;
                }
            }

            TradeEntries.TryAdd(EntryUniqueIdCounter, tradeEntry);
            NewTradeReceived();
        }
    }

    public class TradeEntry
    {
        public TradeEntry(string itemName, string playerNick, string currencyType, string currencyAmount,
            bool incomingTrade, int uniqueId, string rawMessage)
        {
            ItemName = itemName;
            PlayerNick = playerNick;
            CurrencyType = currencyType;
            CurrencyAmount = currencyAmount;
            IsIncomingTrade = incomingTrade;
            UniqueId = uniqueId;
            RawMessage = rawMessage;
            Timestamp = DateTime.Now;
        }

        public string PlayerNick { get; }
        public string ItemName { get; }
        public string CurrencyType { get; }
        public string CurrencyAmount { get; }
        public bool IsIncomingTrade { get; }
        public DateTime Timestamp { get; }
        public ItemPosInfo ItemPosInfo { get; set; }
        public int UniqueId { get; }
        public string RawMessage { get; }
        public string OfferText { get; set; } = string.Empty;
    }

    public class ItemPosInfo
    {
        public ItemPosInfo(string tabName, Vector2 pos)
        {
            TabName = tabName;
            Pos = pos;
        }

        public string TabName { get; }
        public Vector2 Pos { get; }
    }
}