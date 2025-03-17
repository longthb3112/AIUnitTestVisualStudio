using OpenAI;
using OpenAI.Chat;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class OpenAIService
{
    public OpenAIService(IConfiguration configuration)
    {
    }

    public async Task<string> GenerateUnitTestAsync(string classCode, string apiKey, string model)
    {
        var prompt = $"Generate test cases for the following C# class. The unit tests should use Moq to mock any database or any dependencies, " +
            $"include positive and negative test cases, and verify the expected behavior. Ensure proper assertions and exception handling. " +
            $"namespace should be the same with C# class :\n\n{classCode} and return only C# code and remove ```csharp and ```";
        OpenAIClient client = new OpenAIClient(apiKey);

        var chatRequest = new ChatRequest(
            model: model,
            messages: new[]
            {
            new Message(Role.System, "You are an AI expert in unit test that generates unit tests using NUnit and Moq for C# classes."),
            new Message(Role.User, prompt)
            },
            temperature: 0.5,
            maxTokens: 1000
        );
        try
        {
            var response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);

            return response?.Choices?.FirstOrDefault()?.Message?.Content.GetString() ?? "No response from AI.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

}
