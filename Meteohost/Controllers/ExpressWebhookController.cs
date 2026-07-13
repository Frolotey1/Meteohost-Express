using Microsoft.AspNetCore.Mvc;
using Meteohost.Models;
using Meteohost.Services;

namespace Meteohost.Controllers;

public class ExpressWebhookController : ControllerBase
{
    private readonly IExpressService _expressService;
    private readonly ILogger<ExpressWebhookController> _logger;

    public ExpressWebhookController(
        IExpressService expressService,
        ILogger<ExpressWebhookController> logger
    )
    {
        _expressService = expressService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>),200)]
    [ProducesResponseType(typeof(ApiResponse<object>),400)]
    [ProducesResponseType(typeof(ApiResponse<object>),500)]
    public async Task<IActionResult> ReceiveMessage([FromBody] ExpressWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Получено сообщение с Express");

            if(string.IsNullOrEmpty(request.Message))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = "Требуется сообщение",
                    ErrorCode = "MISSING_MESSAGE"
                });
            }

            var result = await _expressService.ProcessWebhookAsync(request);

            if(result)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Webhook действует успешно",
                    Data = new
                    {
                        received = request.Message,
                        from = request.SenderId,
                        chat = request.ChatId,
                        timestamp = DateTime.UtcNow
                    }
                });
            }

            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Error = "Не получилось подключиться к Express",
                ErrorCode = "PROCESS_ERROR"

            });

        } catch (Exception ex)
        {
            _logger.LogError(ex,"Ошибка считывания сообщения с Express");
            return StatusCode(500,new ApiResponse<object>
            {
                Success = false,
                Error = ex.Message,
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    [HttpGet("test")]
    [ProducesResponseType(typeof(ApiResponse<object>),200)]
    public IActionResult Test()
    {
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Endtpoint для Express работает",
            Data = new
            {
                timestamp = DateTime.UtcNow,
                status = "ready",
                endpoint = "/api/webhook/express"
            }
        });
    }
}