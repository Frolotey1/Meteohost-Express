namespace ExpressGateway.Services;

using ExpressGateway.Models;

public interface IExpressService
{
    Task<SendMessageResponse> SendMessageAsync(string chatId, string message, string? asset = null);
    Task<SendMessageResponse> SendToDefaultGroupAsync(string message);
    Task<IEnumerable<ChatInfo>> GetChatsAsync();
    Task<bool> ProcessWebhookAsync(WebhookRequest request);
    Task<bool> SendIssueAsync(string issueText);
}

public class ChatInfo
{
    public string Asset { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
}
