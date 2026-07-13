using MeteoLib;
using MeteoLib.Impl.Delivery;
using MeteoLib.Interfaces;
using Meteohost.Services;
using Microsoft.AspNetCore.Mvc;
using Meteohost.Models;

namespace Meteohost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProofController : ControllerBase
    {
        private readonly Delivery _delivery;
        private readonly IIssueTracker _issueTracker;
        private readonly IExpressService _expressService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProofController> _logger;

        public ProofController(
            Delivery delivery, 
            IIssueTracker issueTracker,
            IExpressService expressService,
            IConfiguration configuration,
            ILogger<ProofController> logger)
        {
            _delivery = delivery;
            _issueTracker = issueTracker;
            _expressService = expressService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("delivery")]
        public IActionResult Delivery(string asset, Trigger trigger)
        {
            try
            {
                _delivery.Send(asset, trigger);
                return Ok(new {success = true,message = "Delivery - отправка", asset = ""});
            } catch (Exception ex)
            {
               return StatusCode(500,new {error = ex.Message });
            }
        }

        [HttpPost("send-default")]
        public async Task<IActionResult> SendToDefaultGroup([FromBody] string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return BadRequest(new { error = "Message is required" });

                var result = await _expressService.SendToDefaultGroupAsync(message);

                return Ok(new
                {
                 success = result.Success,
                    message = result.Success ? "Message sent to default group" : "Failed",
                    chatId = "5455c9c9-3dc6-590b-be34-74f82f46308e",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendToDefaultGroup");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("send-asset")]
        public async Task<IActionResult> SendToAsset([FromBody] SendToAssetRequest request)
        {   
            try
            {
                if (string.IsNullOrEmpty(request.Asset))
                    return BadRequest(new { error = "Asset is required" });

                if (string.IsNullOrEmpty(request.Message))
                    return BadRequest(new { error = "Message is required" });

                var result = await _expressService.SendToAssetAsync(request.Asset, request.Message);

            return Ok(new
            {
                success = result.Success,
                message = result.Success ? "Message sent to asset" : result.Error,
                asset = request.Asset,
                chatId = result.ChatId,
                timestamp = DateTime.UtcNow
            });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendToAsset");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("issue")]
        public IActionResult Issue(string text)
        {
            try
            {
                _issueTracker.AddIssue(text);
                return Ok(new {success = true,message = "Отправка issue информации"});
            } catch (Exception ex)
            {
                return StatusCode(500,new {error = ex.Message });
            }
        }

        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<ExpressMessageResponse>),200)]
        [ProducesResponseType(typeof(ApiResponse<object>),400)]
        [ProducesResponseType(typeof(ApiResponse<object>),500)]
        public async Task<IActionResult> SendMessage([FromBody] ExpressSendRequest request)
        {
            try
            {
                if(string.IsNullOrEmpty(request.ChatId))
                    return BadRequest(new {error = "Требуется ChatId"});
                
                if(string.IsNullOrEmpty(request.Message))
                    return BadRequest(new {error = "Требуется сообщение (message)"});

                var result = await _expressService.SendMessageAsync(
                    request.ChatId,
                    request.Message,
                    request.Asset
                );

                if(result.Success)
                {
                    return Ok(new 
                    {success = true, 
                    message = "Сообщение отправлено",
                    chatId = request.ChatId,
                    timestamp = DateTime.UtcNow
                    });
                }

                return StatusCode(500,new {error = result.Error});
                
            } catch (Exception ex)
            {
                _logger.LogError(500,"Ошибка отправки сообщения. Сигнал error");
                return StatusCode(500,new {error = ex.Message});
            }
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestMessage([FromBody] string chatId)
        {
            try
            {
                if(string.IsNullOrEmpty(chatId))
                    return BadRequest(new {error = "Требуется ChatId"});

                var testMessage = $"Тестирование API с {DateTime.UtcNow:HH::mm::ss} UTC";
                var result = await _expressService.SendMessageAsync(chatId,testMessage);

                return Ok(new
                {
                    success = result.Success,
                    message = result.Success ? "Тестовое сообщение отправлено" : "Тест провалился",
                });

            } catch (Exception ex)
            {
                _logger.LogError(500,"Тестирование сообщения завершилось неудачно");
                return StatusCode(500, new {error = ex.Message});
            }
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetChats()
        {
            try
            {
                var chats = await _expressService.GetChatsAsync();
                return Ok(new
                {
                    success = true,
                    count = chats.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка извлечения всего списка чатов");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                success = true,
                message = "pong",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Неизвестно"
            });
        }

        [HttpPost("send-test-redmine")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> SendToTestRedmine([FromBody] string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return BadRequest(new { error = "Message is required" });

                var result = await _expressService.SendToTestRedmineAsync(message);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Сообщение отправлено в test_redmine",
                        chatId = "9036c1e4-c02d-58cf-bab3-8413c1e7a680",
                        timestamp = DateTime.UtcNow
                    });
                }

                return StatusCode(500, new { error = result.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки в test_redmine");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            try
            {
                var section = _configuration.GetSection("ExpressProperties");
                var bot = section.GetValue<string>("bot");
                var groups = section.GetSection("groups").Get<Dictionary<string, string>[]>();

                return Ok(new
                {
                    success = true,
                    botId = bot?.Split(':')[0] ?? "Неизвестно",
                    chatCount = groups?.Length ?? 0,
                    version = "1.0.0",
                    status = "active"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetInfo");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
