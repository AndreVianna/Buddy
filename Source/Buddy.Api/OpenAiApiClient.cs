namespace Buddy.Api;

public class OpenAiApiClient
{
    private readonly OpenAIAPI _api;

    public OpenAiApiClient(string apiKey)
    {
        _api = new OpenAIAPI(apiKey);
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        var chat = await _api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.1,
            MaxTokens = 50,
            Messages = new ChatMessage[]
            {
                new ChatMessage(ChatMessageRole.User, prompt)
            }
        });

        var reply = chat.Choices[0].Message;
        return $"{reply.Role}: {reply.Content.Trim()}";
    }
}

