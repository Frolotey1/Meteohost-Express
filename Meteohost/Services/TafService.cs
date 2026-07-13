using MeteoLib;
using MeteoLib.Interfaces;
using MeteoLib.LoadService;

namespace Meteohost.Services
{
    internal class TafService : BackgroundService
    {
        readonly ILogger _logger;
        private readonly IAirports _airports;
        private readonly LoadService _loadService;
        public static TimeSpan DELAY = TimeSpan.FromMinutes(1);

        public TafService(IAirports airports, LoadService loadService, ILogger<MetarService> logger)
        {
            _logger = logger;
            _airports = airports;
            _loadService = loadService;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var arr = _airports.CodeIataList.ToList();

            _logger.LogInformation("Taf integration started [{IataList}]", string.Join(',', arr));


            var tasks = arr.Select(a => Task.Run(() => StartAirportAsync(a, stoppingToken)));



            await Task.WhenAll(tasks);
        }



        private async Task StartAirportAsync(string iata, CancellationToken cancellationToken)
        {

            while (true)
                try
                {
                    await _loadService.LoadTafAsync(iata);

                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(DELAY, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (MeteoException ex)
                {
                    _logger.LogWarning(ex, "Taf api error {Iata}", iata);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Taf unhandled error, wait 10 mins {Iata}", iata);
                    await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
                }
        }
    }
}