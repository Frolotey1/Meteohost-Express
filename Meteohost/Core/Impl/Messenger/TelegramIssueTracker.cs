using MeteoLib.Interfaces;
using Telegram.Bot;

namespace Meteohost.Core.Impl.Messenger
{
    public class TelegramIssueTracker:IIssueTracker
    {
        private readonly string _botToken;
        private readonly string _chatToken;
        private TelegramBotClient _botClient = null!;

        public TelegramIssueTracker(string botToken, string chatToken)
        {
            ArgumentNullException.ThrowIfNull(botToken,nameof(botToken));
            ArgumentNullException.ThrowIfNull(chatToken, nameof(chatToken));

            _botToken = botToken;
            _chatToken = chatToken;
        }


        private TelegramBotClient BotClient
        {
            get
            {
                if (_botClient is null)
                {
                    Interlocked.CompareExchange(ref _botClient, InitBotClient(), null);
                }

                return _botClient;
            }
        }

        public void AddIssue(string issueText)
        {
            BotClient.SendTextMessageAsync(_chatToken, issueText).Wait();
        }

        private TelegramBotClient InitBotClient()
        {
            return new TelegramBotClient(_botToken);
        }
    }
}
