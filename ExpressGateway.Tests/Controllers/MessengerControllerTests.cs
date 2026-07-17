using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExpressGateway.Controllers;
using ExpressGateway.Services;
using ExpressGateway.Models;

namespace ExpressGateway.Tests.Controllers;

public class MessengerControllerTests
{
    private readonly Mock<IExpressService> _mockService;
    private readonly Mock<ILogger<MessengerController>> _mockLogger;
    private readonly MessengerController _controller;

    public MessengerControllerTests()
    {
        _mockService = new Mock<IExpressService>();
        _mockLogger = new Mock<ILogger<MessengerController>>();
        
        _controller = new MessengerController(
            _mockService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Send_WithValidRequest_ShouldReturnOk()
    {
        var request = new SendMessageRequest
        {
            ChatId = "chat_123",
            Message = "Test message",
            Asset = "test-asset"
        };

        var expectedResponse = new SendMessageResponse
        {
            Success = true,
            MessageId = "msg_456",
            SentAt = DateTime.UtcNow
        };

        _mockService
            .Setup(s => s.SendMessageAsync(request.ChatId, request.Message, request.Asset))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Send(request);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockService.Verify(s => s.SendMessageAsync(request.ChatId, request.Message, request.Asset), Times.Once);
    }

    [Fact]
    public async Task Send_WhenServiceThrowsException_ShouldReturn500()
    {
        var request = new SendMessageRequest
        {
            ChatId = "chat_123",
            Message = "Test message"
        };

        _mockService
            .Setup(s => s.SendMessageAsync(request.ChatId, request.Message, request.Asset))
            .ThrowsAsync(new Exception("Service error"));

        var result = await _controller.Send(request);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task GetChats_ShouldReturnChatList()
    {
        var expectedResponse = new ChatListResponse
        {
            Chats = new List<ChatInfo>
            {
                new() { Id = "chat1", Name = "Team Chat", MembersCount = 5 },
                new() { Id = "chat2", Name = "Project Group", MembersCount = 3 }
            },
            Total = 2
        };

        _mockService
            .Setup(s => s.GetChatsAsync(50, 0))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetChats(50, 0);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockService.Verify(s => s.GetChatsAsync(50, 0), Times.Once);
    }

    [Fact]
    public async Task GetChats_WithCustomLimitAndOffset_ShouldReturnChatList()
    {
        var expectedResponse = new ChatListResponse
        {
            Chats = new List<ChatInfo>
            {
                new() { Id = "chat1", Name = "Team Chat", MembersCount = 5 }
            },
            Total = 1
        };

        _mockService
            .Setup(s => s.GetChatsAsync(10, 5))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetChats(10, 5);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockService.Verify(s => s.GetChatsAsync(10, 5), Times.Once);
    }

    [Fact]
    public async Task SetWebhook_WithValidRequest_ShouldReturnOk()
    {
        var request = new WebhookRequest
        {
            Url = "https://my-service.com/webhook",
            Secret = "test-secret"
        };

        var expectedResponse = new WebhookResponse
        {
            Status = "ok",
            Message = "Webhook set successfully"
        };

        _mockService
            .Setup(s => s.SetWebhookAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.SetWebhook(request);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockService.Verify(s => s.SetWebhookAsync(request), Times.Once);
    }

    [Fact]
    public async Task SetWebhook_WhenServiceThrowsException_ShouldReturn500()
    {
        var request = new WebhookRequest
        {
            Url = "https://my-service.com/webhook"
        };

        _mockService
            .Setup(s => s.SetWebhookAsync(request))
            .ThrowsAsync(new Exception("Webhook error"));

        var result = await _controller.SetWebhook(request);

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Ping_ShouldReturnPong()
    {
        var expectedResponse = new PingResponse
        {
            Status = "ok",
            Message = "pong"
        };

        _mockService
            .Setup(s => s.PingAsync())
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Ping();

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockService.Verify(s => s.PingAsync(), Times.Once);
    }

    [Fact]
    public async Task Ping_WhenServiceThrowsException_ShouldReturn500()
    {
        _mockService
            .Setup(s => s.PingAsync())
            .ThrowsAsync(new Exception("Ping error"));

        var result = await _controller.Ping();

        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
