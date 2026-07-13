using MeteoLib;
using MeteoLib.Impl.Delivery;
using MeteoLib.Interfaces;
using MeteoLib.LoadService;
using System.Linq.Expressions;
using System.Text;

namespace Meteohost.Services
{
    public class DeliveryService : BackgroundService
    {

        private readonly IRepo _repo;
        private readonly Delivery _delivery;
        private readonly ILogger _logger;

        private readonly static TimeSpan CHECK_DELAY = TimeSpan.FromSeconds(5);
        private readonly IIssueTracker _releaseTracker;

        public DeliveryService(ILogger<DeliveryService> logger, IRepo repo, Delivery delivery, IIssueTracker releaseTracker)
        {
            _logger = logger;
            _repo = repo;
            _delivery = delivery;
            _releaseTracker = releaseTracker;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("delivery message service started");

            while (true)
            {
                try
                {
                    var assets = _repo.Unprocessed();




                    if (!assets.Any())
                    {
                        await Task.Delay(CHECK_DELAY, stoppingToken);
                        continue;
                    }


                    foreach (var asset in assets)
                    {  
                        CheckAndDeliveryAsset(asset);
                    }

                    stoppingToken.ThrowIfCancellationRequested();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message);
                }
            }
        }

        public void CheckAndDeliveryAsset(string asset)
        {

            var packet = _repo.GetPacket(asset);

            if (packet is null || packet.IsTriggerComplete) return;


            var trigger = packet.Trigger;

            if (trigger is not null && trigger.DeliveryStatus())
            {

                SendTrigger(asset, trigger);
            }

            _repo.TriggerComplete(packet.SN);
        }

        private void SendTrigger(string asset, Trigger trigger)
        {
            try
            {
                _delivery.Send(asset, trigger);
            }
            catch (Exception e)
            {
                var b = ReleaseIssue($"SendTrigger {asset}", trigger, e);

                if (!b) _logger.LogError("{ASSET} : cannot send bug issue ", asset);

                throw;
            }
        }

        private bool ReleaseIssue(params object[] args)
        {
            var sb = new StringBuilder();

            foreach (var item in args)
            {
                sb.AppendLine(item.ToString());
                sb.AppendLine("-------------");
            };

            try
            {
               
                _releaseTracker.AddIssue(sb.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}