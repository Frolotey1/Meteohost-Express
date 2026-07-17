using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using ExpressGateway.Models;

namespace ExpressGateway.Tests.Models;

public class ModelValidationTests
{
    [Fact]
    public void SendMessageRequest_WithValidData_ShouldPassValidation()
    {
        var request = new SendMessageRequest
        {
            ChatId = "chat_123",
            Message = "Hello World"
        };

        var validationResults = ValidateModel(request);
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void SendMessageRequest_WithoutChatId_ShouldFailValidation()
    {
        var request = new SendMessageRequest
        {
            Message = "Hello World"
        };

        var validationResults = ValidateModel(request);

        validationResults.Should().NotBeEmpty();
        validationResults[0].ErrorMessage.Should().Contain("ChatId");
    }

    [Fact]
    public void SendMessageRequest_WithEmptyText_ShouldFailValidation()
    {
        var request = new SendMessageRequest
        {
            ChatId = "chat_123",
            Message = ""
        };

        var validationResults = ValidateModel(request);

        validationResults.Should().NotBeEmpty();
        validationResults[0].ErrorMessage.Should().Contain("Text");
    }

    private List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}