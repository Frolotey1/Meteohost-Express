using MeteoLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Meteohost.Core.Impl.Messenger
{
    public class TelegramMessenger : IMessenger
    {
        private readonly string _token;
        private TelegramBotClient _botClient = null!;
        private readonly Dictionary<string, string> _dictionary = new();

        public TelegramMessenger(string token)
        {
            _token = token;
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

        private TelegramBotClient InitBotClient()
        {
            return new TelegramBotClient(_token);
        }

        private string GetChat(string asset)
        {
            return _dictionary[asset.ToLower()];
        }

        public void AddChat(string asset, string chat)
        {
            ArgumentNullException.ThrowIfNull(asset, nameof(asset));
            ArgumentNullException.ThrowIfNull(chat, nameof(chat));

            _dictionary.Add(asset, chat);
        }

        public void Send(string asset, string message)
        {
            var chat = GetChat(asset);

            BotClient.SendTextMessageAsync(chat, message, ParseMode.MarkdownV2).Wait();
        }
    }
}
