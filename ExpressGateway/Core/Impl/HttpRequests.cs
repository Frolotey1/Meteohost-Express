namespace ExpressGateway.Core.Impl;

using System.Text.Json;

public static class HttpRequests
{
    public record JwtResult(string status, string result);

    public static JwtResult GetJwt(string botId, string signature)
    {
        try
        {
            using var client = new HttpClient();
            var url = $"https://x.ar-management.ru/api/v2/botx/bots/{botId}/token?signature={signature}";
            Console.WriteLine($"[JWT] Request: {url}");

            using var response = client.GetAsync(url).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"[JWT] Status: {response.StatusCode}");
            Console.WriteLine($"[JWT] Content: {content}");

            var json = JsonSerializer.Deserialize<JwtResult>(content);
            if (json == null || string.IsNullOrEmpty(json.status))
                return new JwtResult("ERROR", "Failed to parse response");

            return json.status.ToUpper() == "OK" 
                ? new JwtResult("OK", json.result) 
                : new JwtResult("ERROR", json.result ?? "Unknown error");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JWT] ERROR: {ex.Message}");
            return new JwtResult("ERROR", ex.Message);
        }
    }

    public static string SendMessage(string message, string chatId, string jwt)
    {
        try
        {
            using var client = new HttpClient();
            var payload = new { group_chat_id = chatId, notification = new { body = message } };
            var content = JsonContent.Create(payload);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

            Console.WriteLine($"[Send] ChatId: {chatId}");
            Console.WriteLine($"[Send] Message: {message}");

            using var response = client.PostAsync(
                "https://x.ar-management.ru/api/v4/botx/notifications/direct", 
                content).Result;
            
            var result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"[Send] Status: {response.StatusCode}");
            Console.WriteLine($"[Send] Response: {result}");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Send] ERROR: {ex.Message}");
            return $"ERROR: {ex.Message}";
        }
    }
}
