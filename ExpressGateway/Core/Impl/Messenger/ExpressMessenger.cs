namespace ExpressGateway.Core.Impl.Messenger;

using System.Security.Cryptography;
using System.Text;

public class ExpressMessenger
{
    private readonly string _chatId;
    private readonly string _botId;
    private readonly string _secretKey;
    private readonly Dictionary<string, string> _chatMap = new();

    public ExpressMessenger(string chatId, string botId)
    {
        _chatId = chatId;
        var parts = botId.Split(':');
        _botId = parts[0];
        _secretKey = parts.Length > 1 ? parts[1] : string.Empty;
    }

    public string Send(string asset, string message)
    {
        var chat = string.IsNullOrEmpty(asset) || !_chatMap.TryGetValue(asset.ToLower(), out var mapped)
            ? _chatId
            : mapped;

        var signature = GenerateSignature(_botId, _secretKey);
        var jwt = HttpRequests.GetJwt(_botId, signature);

        if (jwt.status.ToUpper() != "OK" || string.IsNullOrEmpty(jwt.result))
        {
            return $"JWT ERROR: {jwt.status}";
        }

        var finalMessage = string.IsNullOrEmpty(asset) ? message : $"({asset}) {message}";
        return HttpRequests.SendMessage(finalMessage, chat, jwt.result);
    }

    public void AddChat(string asset, string chatId)
    {
        _chatMap[asset.ToLower()] = chatId;
    }

    private static string GenerateSignature(string botId, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(botId);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
    }
}
