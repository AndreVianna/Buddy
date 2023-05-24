using System;

using Microsoft.VisualBasic;

namespace Buddy.Api.Services;

public class OpenAiApiClient
{
    private readonly OpenAIAPI _api;
    private readonly Dictionary<Guid, List<ChatMessage>> _conversations;

    public OpenAiApiClient(string apiKey)
    {
        _api = new OpenAIAPI(apiKey);
        _conversations = new Dictionary<Guid, List<ChatMessage>>();
    }

    public async Task<CreateChatResult> CreateAsync(string? system)
    {
        var chatId = Guid.NewGuid();
        system = string.IsNullOrWhiteSpace(system) ? "You are a general assistant to the user." : system;
        _conversations[chatId] = new List<ChatMessage> { new ChatMessage(ChatMessageRole.System, system) };

        var request = CreateChatRequest(chatId);
        var chat = await _api.Chat.CreateChatCompletionAsync(request);
        var reply = chat.Choices[0].Message;
        return new CreateChatResult(chatId, reply.Role.ToString(), reply.Content.Trim());
    }

    public async Task<CreateChatResult?> SendAsync(Guid chatId, string prompt)
    {
        if (!_conversations.ContainsKey(chatId))
        {
            return null;
        }

        _conversations[chatId].Add(new ChatMessage(ChatMessageRole.User, prompt));

        var request = CreateChatRequest(chatId);
        var chat = await _api.Chat.CreateChatCompletionAsync(request);
        var reply = chat.Choices[0].Message;

        _conversations[chatId].Add(new ChatMessage(reply.Role, reply.Content.Trim()));

        return new CreateChatResult(chatId, reply.Role.ToString(), reply.Content.Trim());
    }

    private ChatRequest CreateChatRequest(Guid chatId) =>
        new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.1,
            MaxTokens = 50,
            Messages = _conversations[chatId]
        };
}

