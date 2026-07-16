namespace ExpressGateway.Services;

using ExpressGateway.Core.Impl.Messenger;
using ExpressGateway.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class ExpressService : IExpressService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpressService> _logger;
    private readonly Dictionary<string, ChatInfo> _chatCache = new();
    private readonly string _defaultChatId;

    public ExpressService(IConfiguration configuration, ILogger<ExpressService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _defaultChatId = _configuration["ExpressSettings:DefaultChatId"] 
            ?? throw new Exception("DefaultChatId not configured");
        LoadChats();
    }

    private void LoadChats()
    {
        var section = _configuration.GetSection("ExpressSettings:Chats");
        var groups = section.GetChildren();

        foreach (var group in groups)
        {
            var asset = group.Key;
            var chatId = group.Value;
            
            _chatCache[asset.ToLower()] = new ChatInfo
            {
                Asset = asset,
                ChatId = chatId,
                Name = $"Chat for {asset}",
                IsDefault = chatId == _defaultChatId
            };
        }

        if (!_chatCache.ContainsKey("default"))
        {
            _chatCache["default"] = new ChatInfo
            {
                Asset = "default",
                ChatId = _defaultChatId,
                Name = "Default Group",
                IsDefault = true
            };
        }
    }

    public async Task<SendMessageResponse> SendMessageAsync(string chatId, string message, string? asset = null)
    {
        try
        {
            _logger.LogInformation("Sending to Express: ChatId={ChatId}, Asset={Asset}", chatId, asset);

            var botId = _configuration["ExpressSettings:BotId"]
                ?? throw new Exception("BotId not configured");

            var messenger = new ExpressMessenger(chatId, botId);
            var result = messenger.Send(asset ?? "", message);

            return new SendMessageResponse
            {
                Success = true,
                ChatId = chatId,
                SentAt = DateTime.UtcNow,
                Status = "sent",
                MessageId = Guid.NewGuid().ToString(),
                ExpressResponse = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            return new SendMessageResponse
            {
                Success = false,
                Error = ex.Message,
                ChatId = chatId
            };
        }
    }

    public async Task<SendMessageResponse> SendToDefaultGroupAsync(string message)
    {
        return await SendMessageAsync(_defaultChatId, message);
    }

    public async Task<IEnumerable<ChatInfo>> GetChatsAsync()
    {
        return _chatCache.Values;
    }

    public async Task<bool> ProcessWebhookAsync(WebhookRequest request)
    {
        _logger.LogInformation("Webhook received: {Message}", request.Message);
        return true;
    }

    public async Task<bool> SendIssueAsync(string issueText)
    {
        _logger.LogInformation("Issue: {Issue}", issueText);
        return true;
    }
}
