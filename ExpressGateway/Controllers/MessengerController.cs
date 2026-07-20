using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExpressGateway.Services;
using ExpressGateway.Models;

namespace ExpressGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessengerController : ControllerBase
{
    private readonly IExpressService _expressService;
    private readonly ILogger<MessengerController> _logger;

    public MessengerController(IExpressService expressService, ILogger<MessengerController> logger)
    {
        _expressService = expressService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
    {
        try
        {
            var result = await _expressService.SendMessageAsync(request.ChatId, request.Message, request.Asset);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("send-default")]
    public async Task<IActionResult> SendDefault([FromBody] SendDefaultMessageRequest request)
    {
        try
        {
            var result = await _expressService.SendToDefaultGroupAsync(request.Message);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending default message");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("chats")]
    public async Task<IActionResult> GetChats([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var result = await _expressService.GetChatsAsync(limit, offset);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chats");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            var result = await _expressService.PingAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ping");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}