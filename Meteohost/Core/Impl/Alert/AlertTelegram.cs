using MeteoLib.Interfaces;
using Telegram.Bot;

namespace Meteohost.Core.Impl.Alert
{
    public class AlertTelegram : IAlert
    {
        private TelegramBotClient _botClient = null!;
        private readonly string _botToken;
        private readonly string _chatToken;
        public AlertTelegram(string botToken, string chatToken)
        {
            ArgumentNullException.ThrowIfNull(botToken, nameof(botToken));
            ArgumentNullException.ThrowIfNull(chatToken, nameof(chatToken));

            _botToken = botToken;
            _chatToken = chatToken;
            _botClient = InitBotClient();
        }
        private TelegramBotClient InitBotClient()
        {
            return new TelegramBotClient(_botToken);
        }
        public async Task SendAsync(string issueText)
        {
            await _botClient.SendTextMessageAsync(_chatToken, issueText);
        }
    }
}
