using MeteoLib.Interfaces;

namespace Meteohost.Core.Impl.Messenger
{
    public class NullMessenger : IMessenger, IFlttMessenger, IIssueTracker
    {
        private static readonly Lazy<NullMessenger> _instance = new(() => new NullMessenger());


        public static NullMessenger Instance => _instance.Value;

        public void AddIssue(string issueText)
        {
            // do nothing
        }

        public void Send(string asset, string message)
        {
            // do nothing
        }
    }
}
