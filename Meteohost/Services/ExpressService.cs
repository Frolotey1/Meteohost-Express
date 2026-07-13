namespace Meteohost.Services;

using Meteohost.Core.Impl.Messenger;
using Meteohost.Models;
using MeteoLib.Interfaces;

public class ExpressService : IExpressService
{
    private readonly IConfiguration _configuration;
    private readonly IIssueTracker _issueTracker;
    private readonly ILogger<ExpressService> _logger;
    private readonly Dictionary<string,ChatInfo> _chatCache = new();
    private readonly string _defaultChatId = "5455c9c9-3dc6-590b-be34-74f82f46308e";

    public ExpressService(IConfiguration configuration, IIssueTracker issueTracker ,ILogger<ExpressService> logger)
    {
        _configuration = configuration;
        _issueTracker = issueTracker;
        _logger = logger;
        LoadChats();
    }

    private void LoadChats()
    {
        var section = _configuration.GetSection("ExpressProperties");
        var groups = section.GetSection("groups").Get<Dictionary<string, string>[]>();

        if(groups != null)
        {
            foreach(var group in groups.SelectMany(g => g))
            {
                _chatCache[group.Key.ToLower()] = new ChatInfo
                {
                    Asset = group.Key,
                    ChatId = group.Value
                };
            }
        }

        if(!_chatCache.ContainsKey("default"))
        {
            _chatCache["default"] = new ChatInfo
            {
                Asset = "default",
                ChatId = _defaultChatId,
                Name = "Express группа"
            };
        }
    }

    public async Task<ExpressMessageResponse> SendMessageAsync(string chatId, string message, string? asset = null)
    {
        try
        {
            _logger.LogInformation("Отправка сообщений в Express: ChatId{chatId}, Asset={Asset}",chatId,asset);

            var botSection = _configuration.GetRequiredSection("ExpressProperties");
            var botId = botSection.GetValue<string>("bot");

            if(string.IsNullOrWhiteSpace(botId))
            {
                return new ExpressMessageResponse
                {
                    Success = false,
                    Error = "ID бота не сконфигурирован"
                };
            }

            var express = new ExpressMessenger(chatId,botId);
            express.Send(asset ?? "",message);

            return new ExpressMessageResponse
            {
                Success = true,
                ChatId = chatId,
                SendAt = DateTime.UtcNow,
                Status = "sent",
                MessageId = Guid.NewGuid().ToString()
            };

        } catch (Exception ex)
        {
            _logger.LogError(ex,"Не удалось отправить сообщение в Express");

            return new ExpressMessageResponse
            {
                Success = false,
                Error = ex.Message,
                ChatId = chatId
            };
        }
    }

    public async Task<ExpressMessageResponse> SendToDefaultGroupAsync(string message, string? asset = null)
    {
        return await SendMessageAsync(_defaultChatId,message,asset);
    }

    public async Task<ExpressMessageResponse> SendToAssetAsync(string asset, string message)
    {
        var chatId = GetChatIdByAsset(asset);
        if(string.IsNullOrEmpty(chatId))
        {
            return new ExpressMessageResponse
            {
                Success = false,
                Error = $"Asset: {asset} не найден",
                ChatId = chatId
            };
        }

        return await SendMessageAsync(chatId,message,asset);

    }   

    public string GetChatIdByAsset(string asset)
    {
        var key = asset.ToLower();
        return _chatCache.TryGetValue(key, out var chat) ? chat.ChatId : _defaultChatId;
    }

    public async Task<bool> SendIssueAsync(string issueText)
    {
        try
        {
            _logger.LogInformation("Отправка issue в Express");
            _issueTracker.AddIssue(issueText);
            return true;

        } catch (Exception ex)
        {
            _logger.LogError(ex,"Не удалось отправить issue информацию в Express");
            return false;
        }
    }

    public async Task<IEnumerable<ChatInfo>> GetChatsAsync()
    {
        return _chatCache.Values;
    }

    public async Task<bool> ProcessWebhookAsync(ExpressWebhookRequest request)
    {
        try {
            _logger.LogInformation("Webhook получил данные: {senderId}, Chat={chatId}, Message={Message}",
                request.SenderId,
                request.ChatId,
                request.Message
            );
            return true;
        } catch (Exception ex)
        {
            _logger.LogError(ex,"Не удалось установить взаимодействие с Webhook");
            return false;
        }
    }

    private async Task HandleCommandAsync(ExpressWebhookRequest request)
    {

        var command = request.Message?.TrimStart('/')?.ToLower();

        if(command == null) return;

        switch(command)
        {
            case "help":
                await SendMessageAsync(request.ChatId,"Доступные команды:\n/help - для справки по командан\n/status = статус работы системы\n/ping проверка");
                break;
            case "status":
                await SendMessageAsync(request.ChatId,$"Статус системы активен\nВремя: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                break;
            case "ping":
                await SendMessageAsync(request.ChatId,"Соединение API с Express успешно");
                break;
            default:
                await SendMessageAsync(request.ChatId, 
                    $"Неизвестная команда: {command}\nВведите /help для справки");
                break;
        }
    }

        public async Task<ExpressMessageResponse> SendToTestRedmineAsync(string message)
        {
        try
        {
            _logger.LogInformation("Отправка сообщения в test_redmine");
            
            var testRedmineSection = _configuration.GetSection("ExpressProperties:testRedmine");
            var chatId = testRedmineSection.GetValue<string>("chatId");
            var secretKey = testRedmineSection.GetValue<string>("secretKey");
            
            if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(secretKey))
            {
                return new ExpressMessageResponse
                {
                    Success = false,
                    Error = "test_redmine not configured"
                };
            }
            
            var botSection = _configuration.GetRequiredSection("ExpressProperties");
            var botId = botSection.GetValue<string>("bot");
            
            var express = new ExpressMessenger(chatId, botId, secretKey);
            express.Send("test_redmine", message);
            
            return new ExpressMessageResponse
            {
                Success = true,
                ChatId = chatId,
                SendAt = DateTime.UtcNow,
                Status = "sent",
                MessageId = Guid.NewGuid().ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отправки в test_redmine");
            return new ExpressMessageResponse
            {
                Success = false,
                Error = ex.Message,
                ChatId = "9036c1e4-c02d-58cf-bab3-8413c1e7a680"
            };
        }
    }
}