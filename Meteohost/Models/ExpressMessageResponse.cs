namespace Meteohost.Models;

public class ExpressMessageResponse
{
    public bool Success {get;set;}
    public string? MessageId {get;set;}
    public string? ChatId {get;set;}
    public DateTime? SendAt {get;set;}
    public string? Status {get;set;}
    public string? Error {get;set;}

}