using System;
using System.Text;
using DiscordChatPlugin.Plugins;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Libraries.Placeholders;
using Oxide.Plugins;

namespace DiscordChatPlugin.Helpers
{
    public class DiscordSendQueue
    {
        private readonly StringBuilder _message = new StringBuilder();
        private Timer _sendTimer;
        private readonly Snowflake _channelId;
        private readonly string _templateId;
        private readonly Action _callback;
        private readonly PluginTimers _timer;

        public DiscordSendQueue(Snowflake channelId, string templateId, PluginTimers timers)
        {
            _channelId = channelId;
            _templateId = templateId;
            _callback = Send;
            _timer = timers;
        }

        public void QueueMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
                
            if (_message.Length + message.Length > 2000)
            {
                Send();
            }

            if (_sendTimer == null)
            {
                _sendTimer = _timer.In(1f, _callback);
            }

            _message.AppendLine(message);
        }

        public void SendTemplate(string templateId, PlaceholderData data)
        {
            DiscordChat.Instance.SendGlobalTemplateMessage(templateId, _channelId, data);
        }
        
        public void Send()
        {
            if (_message.Length > 2000)
            {
                _message.Length = 2000;
            }

            PlaceholderData placeholders = DiscordChat.Instance.GetDefault().Add(DiscordChat.ChatMessagePlaceholder, _message.ToString());
            _message.Length = 0;
            DiscordChat.Instance.SendGlobalTemplateMessage(_templateId, _channelId, placeholders);
            _sendTimer?.Destroy();
            _sendTimer = null;
        }
    }
}