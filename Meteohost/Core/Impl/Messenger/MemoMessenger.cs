using MeteoLib.Interfaces;
using System.Collections;

namespace Meteohost.Core.Impl.Messenger
{
    public class MemoMessenger : IFlttMessenger, IMessenger
    {
        private readonly List<string> _messages = new();
        public IEnumerable<string> Messages => _messages;

        public void Send(string asset, string message)
        {
            _messages.Add(message);
        }
    }
}
