using Meteohost.Core.Impl.Messenger;
using MeteoLib.Interfaces;

namespace Meteohost.Core.Impl.Alert
{
    public class AlertExpress : IAlert
    {
        private readonly string _botToken;
        private readonly string _chatToken;
        public AlertExpress(string botToken, string chatToken)
        {
            ArgumentNullException.ThrowIfNull(chatToken, nameof(chatToken));
            ArgumentNullException.ThrowIfNull(botToken, nameof(botToken));

            _botToken = botToken;
            _chatToken = chatToken;
        }
        public async Task SendAsync(string issueText)
        {
            var express = new ExpressMessenger(_chatToken, _botToken);
            express.Send("", issueText);
        }
    }
}
