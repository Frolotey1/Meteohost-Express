using MeteoLib.Interfaces;

namespace Meteohost.Core.Impl.Messenger
{
    public class ConsoleMessenger : IFlttMessenger, IMessenger, IIssueTracker
    {

        private ConsoleMessenger() { }
        private static readonly Lazy<ConsoleMessenger> _instance = new(() => new ConsoleMessenger());

        public static ConsoleMessenger Instance => _instance.Value;

        void IFlttMessenger.Send(string asset, string message) => Send("fltt", asset, message);

        void IMessenger.Send(string asset, string message) => Send("express", asset, message);

        private static void Send(string channel, string asset, string message) => Console.WriteLine($"{channel.ToUpper()}:{asset.ToUpper()}\n------------\n{message}");

        public void AddIssue(string issueText) => Console.WriteLine(issueText);
    }
}