using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PoeTradesHelper
{
    public class ReplyButtonsController
    {
        private const string ConfigFileName = "FastReplyButtons.txt";
        private const string OUTGOING_REPLIES = "OutgoingReplies:";
        private const string INCOMING_REPLIES = "IncomingReplies:";

        public List<ReplyButtonInfo> IncomingReplies = new List<ReplyButtonInfo>();
        public List<ReplyButtonInfo> OutgoingReplies = new List<ReplyButtonInfo>();
        private string _pluginFolder;
        private string FilePath => Path.Combine(_pluginFolder, ConfigFileName);

        public void Load(string pluginFolder)
        {
            _pluginFolder = pluginFolder;

            if (!File.Exists(FilePath))
                SaveDefaults();
            else
                LoadFromFile();
        }

        private void LoadFromFile()
        {
            var lines = File.ReadAllLines(FilePath);

            var parseOutgoing = false;
            var parseIncoming = false;
            foreach (var line in lines)
            {
                var trimedLine = line.Trim();

                if (string.IsNullOrEmpty(trimedLine) || trimedLine.StartsWith("#") || trimedLine.StartsWith("//"))
                    continue;

                if (trimedLine == OUTGOING_REPLIES)
                {
                    parseIncoming = false;
                    parseOutgoing = true;
                    continue;
                }
                if (trimedLine == INCOMING_REPLIES)
                {
                    parseOutgoing = false;
                    parseIncoming = true;
                    continue;
                }

                var splited = trimedLine.Split('|');

                if (splited.Length < 2)
                {
                    continue;
                }

                var kickLeaveParty = splited.Length >= 3 && splited[2].ToUpper() == "X";
                var close = splited.Length >= 3 && splited[2].ToUpper() == "C";
                var goHideout = splited.Length >= 3 && splited[2].ToUpper() == "H";
                var replyInfo = new ReplyButtonInfo(splited[0], splited[1], kickLeaveParty, close, goHideout);

                if(parseIncoming)
                    IncomingReplies.Add(replyInfo);
                else if(parseOutgoing)
                    OutgoingReplies.Add(replyInfo);
            }
        }

        private void SaveDefaults()
        {
            OutgoingReplies.Add(new ReplyButtonInfo("Thanks", "Thanks", true, goToOwnHideout:true));

            IncomingReplies.Add(new ReplyButtonInfo("1m", "one moment please"));
            IncomingReplies.Add(new ReplyButtonInfo("Thanks", "thanks", true));
            IncomingReplies.Add(new ReplyButtonInfo("No thx", "no, thanks", false, true));
            IncomingReplies.Add(new ReplyButtonInfo("Later", "busy atm. I'll whisper you later if you want"));
            IncomingReplies.Add(new ReplyButtonInfo("Sold", "sold", false, true));

            var sb = new StringBuilder();
            sb.AppendLine("// Fast response buttons");
            sb.AppendLine("// Format:");
            sb.AppendLine("// button name|message");
            sb.AppendLine("// or");
            sb.AppendLine("// button name|message|X");
            sb.AppendLine("//");
            sb.AppendLine("// X mean leave party (or kick from own party, after trade) and close the trade element");
            sb.AppendLine("// C mean just close the trade element");
            sb.AppendLine("// H mean leave and go to hideout");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(OUTGOING_REPLIES);

            foreach (var replyButtonInfo in OutgoingReplies)
            {
                var leaveParty = replyButtonInfo.KickLeaveParty ? "|X" : (replyButtonInfo.Close ? "|C" : string.Empty);

                if (replyButtonInfo.GoToOwnHideout)
                    leaveParty = "|H";
                sb.AppendLine($"{replyButtonInfo.ButtonName}|{replyButtonInfo.Message}{leaveParty}");
            }

            sb.AppendLine();
            sb.AppendLine(INCOMING_REPLIES);
            foreach (var replyButtonInfo in IncomingReplies)
            {
                var kickFromParty = replyButtonInfo.KickLeaveParty ? "|X" : (replyButtonInfo.Close ? "|C" : string.Empty);
                sb.AppendLine($"{replyButtonInfo.ButtonName}|{replyButtonInfo.Message}{kickFromParty}");
            }

            File.WriteAllText(FilePath, sb.ToString());
        }
    }

    public class ReplyButtonInfo
    {
        public ReplyButtonInfo(string buttonName, string message, bool kickLeaveParty = false, bool close = false, bool goToOwnHideout = false)
        {
            ButtonName = buttonName;
            Message = message;
            GoToOwnHideout = goToOwnHideout;
            KickLeaveParty = kickLeaveParty;
            Close = close;
        }

        public string ButtonName { get; }
        public string Message { get; }
        public bool Close { get; }
        public bool KickLeaveParty { get; }
        public bool GoToOwnHideout { get; }
    }
}