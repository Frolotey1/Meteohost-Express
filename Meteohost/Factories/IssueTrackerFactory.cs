using Meteohost.Core.Impl.Messenger;
using MeteoLib.Interfaces;

namespace Meteohost.Factories
{
    public class IssueTrackerFactory
    {
        private readonly IConfiguration _configuration;

        public IssueTrackerFactory(IConfiguration  configuration)
        {
            _configuration = configuration;
        }

        internal IIssueTracker GetTracker()
        {
            var mtype = _configuration.GetValue<MessengerType>("IssueTracker");

            return mtype switch
            {
                MessengerType.NULL => NullMessenger.Instance,
                MessengerType.CONSOLE => ConsoleMessenger.Instance,                
                MessengerType.TELEGRAM => InitTelegramMessenger(),
                MessengerType.EXPRESS => InitExpressMessenger(),
                _ => throw new NotImplementedException(mtype.ToString())
            };
        }

        private TelegramIssueTracker InitTelegramMessenger()
        {
            var section = _configuration.GetRequiredSection("IssueTelegramProperties");

            var bot = section.GetValue<string>("bot");
            var chat = section.GetValue<string>("chat");

           
            return new TelegramIssueTracker(bot,chat);

        }
        private ExpressIssueTracker InitExpressMessenger()
        {
            var section = _configuration.GetRequiredSection("IssueExpressProperties");

            var bot = section.GetValue<string>("bot");
            var chat = section.GetValue<string>("chat");


            return new ExpressIssueTracker(bot, chat);

        }
    }
}
