using MeteoLib.Interfaces;

namespace Meteohost.Core.Impl.Messenger
{
    public class HomestubMessenger : IFlttMessenger, IMessenger
    {

        private HomestubMessenger() { }
        private static readonly Lazy<HomestubMessenger> _instance = new(() => new HomestubMessenger());

        public static HomestubMessenger Instance => _instance.Value;

        void IFlttMessenger.Send(string asset, string message) => Send("fltt", asset, message);

        void IMessenger.Send(string asset, string message) => Send("express", asset, message);

        private static void Send(string channel, string asset, string message) => throw new NotImplementedException();

    }
}