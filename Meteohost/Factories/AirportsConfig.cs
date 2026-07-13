namespace Meteohost.Factories
{
    public class AirportsConfig : IAirports
    {
        private readonly IConfiguration _configuration;

        public AirportsConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<string> CodeIataList
        {
            get
            {
                var airports = _configuration.GetRequiredSection("assets").Get<string[]>();

                return airports;
            }
        }
    }
}
