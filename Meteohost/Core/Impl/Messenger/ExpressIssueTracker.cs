using MeteoLib.Interfaces;

namespace Meteohost.Core.Impl.Messenger
{
    public class ExpressIssueTracker:IIssueTracker
    {
        private readonly string _botToken;
        private readonly string _chatToken;

        public ExpressIssueTracker(string botToken, string chatToken)
        {
            ArgumentNullException.ThrowIfNull(botToken,nameof(botToken));
            ArgumentNullException.ThrowIfNull(chatToken, nameof(chatToken));

            _botToken = botToken;
            _chatToken = chatToken;
        }

        public void AddIssue(string issueText)
        {
            var express = new ExpressMessenger(_chatToken, _botToken);
            express.Send("", issueText);
        }

    }
}
