using ExpressGateway.Core.Impl.Messenger;
using ExpressGateway.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ExpressGateway.Services;

public class ExpressService : IExpressService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpressService> _logger;
    private readonly Dictionary<string, ChatInfo> _chatCache = new();
    private readonly string _defaultChatId;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;

    public ExpressService(
        IConfiguration configuration, 
        ILogger<ExpressService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        _defaultChatId = _configuration["ExpressSettings:DefaultChatId"] 
            ?? throw new Exception("DefaultChatId not configured");
        
        _apiBaseUrl = _configuration["ExpressSettings:ApiUrl"] 
            ?? throw new Exception("ApiUrl not configured");
        
        _apiKey = _configuration["ExpressSettings:ApiKey"] 
            ?? throw new Exception("ApiKey not configured");
        
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        
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
                Id = chatId,
                Asset = asset,
                ChatId = chatId,
                Name = $"Chat for {asset}",
                IsDefault = chatId == _defaultChatId,
                MembersCount = 0
            };
        }

        if (!_chatCache.ContainsKey("default"))
        {
            _chatCache["default"] = new ChatInfo
            {
                Id = _defaultChatId,
                Asset = "default",
                ChatId = _defaultChatId,
                Name = "Default Group",
                IsDefault = true,
                MembersCount = 0
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

            var payload = new
            {
                chatId = chatId,
                text = message,
                botId = botId,
                asset = asset ?? ""
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Sending payload: {Payload}", jsonPayload);

            var content = new StringContent(
                jsonPayload,
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("/api/v4/messages", content);

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Response Body: {ResponseBody}", responseBody ?? "[empty]");

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return new SendMessageResponse
                    {
                        Success = true,
                        ChatId = chatId,
                        SentAt = DateTime.UtcNow,
                        Status = "sent",
                        MessageId = Guid.NewGuid().ToString(),
                        ExpressResponse = "Message sent successfully (204 No Content)"
                    };
                }

                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<ExpressApiResponse>(responseBody, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return new SendMessageResponse
                        {
                            Success = true,
                            ChatId = chatId,
                            SentAt = DateTime.UtcNow,
                            Status = "sent",
                            MessageId = apiResponse?.MessageId ?? Guid.NewGuid().ToString(),
                            ExpressResponse = responseBody
                        };
                    }
                    catch (JsonException)
                    {
                        return new SendMessageResponse
                        {
                            Success = true,
                            ChatId = chatId,
                            SentAt = DateTime.UtcNow,
                            Status = "sent",
                            MessageId = Guid.NewGuid().ToString(),
                            ExpressResponse = responseBody
                        };
                    }
                }

                return new SendMessageResponse
                {
                    Success = true,
                    ChatId = chatId,
                    SentAt = DateTime.UtcNow,
                    Status = "sent",
                    MessageId = Guid.NewGuid().ToString(),
                    ExpressResponse = "Message sent successfully"
                };
            }

            return new SendMessageResponse
            {
                Success = false,
                Error = $"API Error: {response.StatusCode} - {responseBody}",
                ChatId = chatId
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

    public async Task<ChatListResponse> GetChatsAsync(int limit = 50, int offset = 0)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/chats?limit={limit}&offset={offset}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var externalChats = JsonSerializer.Deserialize<ChatListResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (externalChats != null && externalChats.Chats.Any())
                {
                    return externalChats;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get chats from external API, using cache");
        }

        var chats = _chatCache.Values
            .Skip(offset)
            .Take(limit)
            .Select(c => new ChatInfo
            {
                Id = c.ChatId,
                Name = c.Name ?? c.Asset,
                MembersCount = c.MembersCount
            })
            .ToList();

        return new ChatListResponse
        {
            Chats = chats,
            Total = _chatCache.Count
        };
    }

    public async Task<PingResponse> PingAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/ping");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PingResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new PingResponse
                {
                    Status = "ok",
                    Message = "pong"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External API ping failed, using local response");
        }

        return new PingResponse
        {
            Status = "ok",
            Message = "pong (local)"
        };
    }
}