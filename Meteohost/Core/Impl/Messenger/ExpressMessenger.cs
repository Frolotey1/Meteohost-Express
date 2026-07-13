using MeteoLib.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Meteohost.Core.Impl.Messenger
{
    public class ExpressMessenger : IMessenger
    {
        private readonly string _chatId;
        private readonly string _botId;
        private readonly string _secKey;
        private readonly Dictionary<string, string> _dictionary = new();

        public ExpressMessenger(string chatId, string botId)
        {
            _botId = botId.Split(':')[0];
            _secKey = botId.Split(":")[1];
            _chatId = chatId;
        }
        
        public ExpressMessenger(string chatId, string botId, string secretKey)
        {
            _botId = botId.Split(':')[0];
            _secKey = secretKey; 
            _chatId = chatId;
        }

        public void Send(string asset, string message)
        {
            try
            {
                string chat;
                if (!string.IsNullOrWhiteSpace(asset) && _dictionary.TryGetValue(asset.ToLower(), out var dictChat))
                {
                    chat = dictChat;
                }
                else
                {
                    chat = _chatId;
                }

                Console.WriteLine($"[Express] Отправка в чат: {chat}");
                Console.WriteLine($"[Express] Сообщение: {message}");
                Console.WriteLine($"[Express] Asset: {asset}");

                var sign = getSignature(_botId, _secKey);
                Console.WriteLine($"[Express] Подпись: {sign.Substring(0, Math.Min(10, sign.Length))}...");

                var jwt = HttpRequests.getJWT(_botId, sign);
                Console.WriteLine($"[Express] JWT статус: {jwt.status}");

                if (jwt.status.ToUpper() != "OK" || string.IsNullOrEmpty(jwt.result))
                {
                    Console.WriteLine($"[Express] ОШИБКА: JWT не получен! status={jwt.status}");
                    return;
                }

                Console.WriteLine($"[Express] JWT получен: {jwt.result.Substring(0, Math.Min(20, jwt.result.Length))}...");

                var finalMessage = $"({asset ?? "default"}) " + message;
                Console.WriteLine($"[Express] Финальное сообщение: {finalMessage}");

                var result = HttpRequests.sendMessage(finalMessage, chat, jwt.result);
                Console.WriteLine($"[Express] Результат отправки: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Express] ОШИБКА: {ex.Message}");
                Console.WriteLine($"[Express] Стек: {ex.StackTrace}");
            }
        }

        public void AddChat(string asset, string chat)
        {
            ArgumentNullException.ThrowIfNull(asset, nameof(asset));
            ArgumentNullException.ThrowIfNull(chat, nameof(chat));
            _dictionary.Add(asset.ToLower(), chat);
        }

        private string getSignature(string botId, string secKey)
        {
            var botIdb = Encoding.UTF8.GetBytes(botId);
            var secKeyb = Encoding.UTF8.GetBytes(secKey);
            using (var hmac = new HMACSHA256(secKeyb))
            {
                var hashb = hmac.ComputeHash(botIdb);
                return BitConverter.ToString(hashb).Replace("-", "").ToUpper();
            }
        }
    }
}
