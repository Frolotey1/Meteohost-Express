using MeteoLib.Interfaces;

namespace Meteohost.Core.Impl.Alert
{
    public class AlertMemo : IAlert
    {
        public List<string> errors = new();
        public Task SendAsync(string issueText)
        {
            errors.Add(issueText);     
            return Task.CompletedTask;
        }
    }
}
