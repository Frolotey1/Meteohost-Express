using System.Text.Json;

namespace Meteohost.Core.Impl
{
    public static class HttpRequests
    {
        public record JSONResultJWT(string status, string result);
        
        public static JSONResultJWT getJWT(string botId, string sign)
        {
            try
            {
                using var client = new HttpClient();
                var url = $"https://x.ar-management.ru/api/v2/botx/bots/{botId}/token?signature={sign}";
                Console.WriteLine($"[JWT] Request URL: {url}");
                
                using var result = client.GetAsync(url).Result;
                var content = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"[JWT] Status: {result.StatusCode}");
                Console.WriteLine($"[JWT] Content: {content}");
                
                var content_json = JsonSerializer.Deserialize<JSONResultJWT>(content);
                if (content_json == null || string.IsNullOrWhiteSpace(content_json.status))
                {
                    return new JSONResultJWT("ERROR", "Failed to parse response");
                }

                if (content_json.status.ToUpper() == "OK")
                {
                    return new JSONResultJWT("OK", content_json.result);
                }
                
                return new JSONResultJWT("ERROR", content_json.result ?? "Unknown error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JWT] ERROR: {ex.Message}");
                return new JSONResultJWT("ERROR", ex.Message);
            }
        }
        
        public static string sendMessage(string message, string chatId, string jwt)
        {
            try
            {
                using var client = new HttpClient();
                var payload = new { group_chat_id = chatId, notification = new { body = message } };
                JsonContent content = JsonContent.Create(payload);
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {jwt}");
                
                Console.WriteLine($"[Send] ChatId: {chatId}");
                Console.WriteLine($"[Send] Message: {message}");
                Console.WriteLine($"[Send] JWT: {jwt.Substring(0, Math.Min(30, jwt.Length))}...");
                
                using var result = client.PostAsync("https://x.ar-management.ru/api/v4/botx/notifications/direct", content).Result;
                var answer = result.Content.ReadAsStringAsync().Result;
                
                Console.WriteLine($"[Send] Status: {result.StatusCode}");
                Console.WriteLine($"[Send] Response: {answer}");
                return answer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Send] ERROR: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }
    }
}
