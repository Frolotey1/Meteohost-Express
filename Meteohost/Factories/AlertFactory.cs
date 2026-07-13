using Meteohost.Core.Impl.Alert;
using MeteoLib.Interfaces;

namespace Meteohost.Factories
{
    public class AlertFactory
    {
        private readonly IConfiguration _configuration;

        public AlertFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal IAlert GetAlert()
        {
            var mtype = _configuration.GetValue<MessengerType>("Alert");

            return mtype switch
            {
                MessengerType.MEMO => new AlertMemo(),
                MessengerType.TELEGRAM => InitTelegramMessenger(),
                MessengerType.EXPRESS => InitExpressMessenger(),
                _ => throw new NotImplementedException(mtype.ToString())
            };
        }

        private AlertTelegram InitTelegramMessenger()
        {
            var section = _configuration.GetRequiredSection("AlertTelegramProperties");

            var bot = section.GetValue<string>("bot");
            var chat = section.GetValue<string>("chat");


            return new AlertTelegram(bot, chat);

        }
        private AlertExpress InitExpressMessenger()
        {
            var section = _configuration.GetRequiredSection("AlertExpressProperties");

            var bot = section.GetValue<string>("bot");
            var chat = section.GetValue<string>("chat");


            return new AlertExpress(bot, chat);

        }
    }
}
