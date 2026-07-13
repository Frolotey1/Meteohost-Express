using Meteohost.Core.Impl.Messenger;
using MeteoLib.Interfaces;

namespace Meteohost.Factories
{
    public partial class MessengerFactory
    {
        private IConnectionFactory _connectionFactory;
        private IConfiguration _configuration;

        public MessengerFactory(IConfiguration configuration, IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _configuration = configuration;
        }

        public IFlttMessenger GetFltt()
        {
            var mtype = GetMessengerType("FlttMessenger");

            return mtype switch
            {
                MessengerType.NULL => NullMessenger.Instance,
                MessengerType.CONSOLE => ConsoleMessenger.Instance,
                MessengerType.HOMESTUB => HomestubMessenger.Instance,
                MessengerType.FLTT => new FlttMessenger(_connectionFactory),
                _ => throw new NotImplementedException(mtype.ToString())
            };
        }

        private MessengerType GetMessengerType(string key)
        {
            return _configuration.GetValue<MessengerType>(key);
        }

        internal IMessenger GetTelegram()
        {
            var mtype = GetMessengerType("Messenger");

            return mtype switch
            {
                MessengerType.NULL => NullMessenger.Instance,
                MessengerType.CONSOLE => ConsoleMessenger.Instance,
                MessengerType.HOMESTUB => HomestubMessenger.Instance,
                MessengerType.TELEGRAM => InitTelegramMessenger(),
                MessengerType.EXPRESS => InitExpressMessenger(),
                _ => throw new NotImplementedException(mtype.ToString())
            };
        }

        private TelegramMessenger InitTelegramMessenger()
        {
            var section = _configuration.GetRequiredSection($"TelegramProperties");
            var bot = section.GetValue<string>("bot");
            var groups = section.GetSection("groups").Get<Dictionary<string, string>[]>();
            var tg = new TelegramMessenger(bot);
            foreach (var gr in groups)
            {
                var g = gr.First();

                tg.AddChat(g.Key, g.Value);
            }
            return tg;
        }
        private ExpressMessenger InitExpressMessenger()
        {
            var section = _configuration.GetRequiredSection($"ExpressProperties");
            var bot = section.GetValue<string>("bot");
            var groups = section.GetSection("groups").Get<Dictionary<string, string>[]>();
            var exp = new ExpressMessenger("", bot);
            foreach (var gr in groups)
            {
                var g = gr.First();

                exp.AddChat(g.Key, g.Value);
            }
            return exp;
        }

    }
}
