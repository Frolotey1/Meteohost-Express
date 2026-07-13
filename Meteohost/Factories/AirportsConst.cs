using Meteohost;

public class AirportsConst : IAirports
{
    public IEnumerable<string> CodeIataList { get; } = new[] { "PKC", "SVX", "KUF", "ROV", "GOJ", "GSV", "REN", "BQS" };
}