using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using ExpressGateway.Models;

namespace ExpressGateway.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<WebhookRequest>>
{
    private readonly WebApplicationFactory<WebhookRequest> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<WebhookRequest> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"status\":\"ok\"");
    }

    [Fact]
    public async Task PingEndpoint_ShouldReturnPong()
    {
        var response = await _client.GetAsync("/api/Messenger/ping");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"message\":\"pong\"");
    }

    [Fact]
    public async Task SendMessageEndpoint_WithValidRequest_ShouldReturnOk()
    {
        var request = new SendMessageRequest
        {
            ChatId = "test_chat",
            Message = "Integration test message"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Messenger/send", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("\"success\":true");
    }
}