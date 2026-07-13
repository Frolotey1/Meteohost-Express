namespace Meteohost.Services;
using Meteohost.Models;

public interface IExpressService
{
    Task<ExpressMessageResponse> SendMessageAsync(string chatId,string message,string? asset = null);
    Task<ExpressMessageResponse> SendToDefaultGroupAsync(string message, string? asset = null);
    Task<ExpressMessageResponse> SendToAssetAsync(string asset, string message);
    string GetChatIdByAsset(string asset);
    Task<bool> SendIssueAsync(string issueText);
    Task<IEnumerable<ChatInfo>> GetChatsAsync();
    Task<bool> ProcessWebhookAsync(ExpressWebhookRequest request);
    Task<ExpressMessageResponse> SendToTestRedmineAsync(string message);
}

public class ChatInfo {
        public string Asset { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public string? Name { get; set; }
}