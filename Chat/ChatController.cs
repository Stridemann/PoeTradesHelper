using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ImGuiNET;

namespace PoeTradesHelper.Chat
{
    public class ChatController
    {
        //private const string LOG_PATH =
        //    @"C:\HomeProjects\Games\_PoE\HUD\PoEHelper\Plugins\Compiled\PoeTradesHelper\chatLog.txt";

        private readonly GameController _gameController;
        private long _lastMessageAddress;
        private readonly Stopwatch _updateSw = Stopwatch.StartNew();
        public event Action<string> MessageReceived = delegate { };
        private readonly Settings _settings;
        public ChatController(GameController gameController, Settings settings)
        {
            _gameController = gameController;
            _settings = settings;
            //File.Delete(LOG_PATH);
            ScanChat(true);
        }
        public void Update()
        {
            if (_updateSw.ElapsedMilliseconds > _settings.ChatScanDelay.Value)
            {
                _updateSw.Restart();
                ScanChat(false);
            }
        }
        private void ScanChat(bool firstScan)
        {
            var messageElements = _gameController.Game.IngameState.IngameUi.ChatBox.Children.ToList();

            var msgQueue = new Queue<string>();
            for (var i = messageElements.Count - 1; i >= 0; i--)
            {
                var messageElement = messageElements[i];

                if (messageElement.Address == _lastMessageAddress)
                    break;

                if (!messageElement.IsVisibleLocal)
                    continue;

                var text = NativeStringReader.ReadStringLong(messageElement.Address + 0x3A0, messageElement.M);
                msgQueue.Enqueue(text);

                //try
                //{
                //    File.AppendAllText(LOG_PATH, $"{text}{Environment.NewLine}");
                //}
                //catch
                //{
                //    //ignored
                //}
            }


            _lastMessageAddress = messageElements.LastOrDefault()?.Address ?? 0;

            if (firstScan)
                return;

            while (msgQueue.Count > 0)
            {
                try
                {
                    MessageReceived(msgQueue.Dequeue());
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"Error processing chat message. Error: {e.Message}", 5);
                }
            }
        }

        public void PrintToChat(string message, bool send = true)
        {
            if (!_gameController.Window.IsForeground())
            {
                WinApi.SetForegroundWindow(_gameController.Window.Process.MainWindowHandle);
            }
            //TODO: Check that chat is opened or no
            SendKeys.SendWait("{ENTER}");
            ImGui.SetClipboardText(message);
            SendKeys.SendWait("^v");

            if(send)
                SendKeys.SendWait("{ENTER}");
            //WinApi.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            //WinApi.SetForegroundWindow(_gameController.Window.Process.MainWindowHandle);
        }
    }
}