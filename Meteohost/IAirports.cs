namespace Meteohost
{
    public interface IAirports
    {
        IEnumerable<string> CodeIataList { get; }
    }
}