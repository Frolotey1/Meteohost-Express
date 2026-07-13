namespace Meteohost.Models;

public class SendToAssetRequest
{
    public string Asset {get;set;} = string.Empty;
    public string Message {get;set;} = string.Empty;
}