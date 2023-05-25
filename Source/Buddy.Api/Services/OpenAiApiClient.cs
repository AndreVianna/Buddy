using Microsoft.Extensions.Logging.Abstractions;

namespace Buddy.Api.Services;

public class OpenAiApiClient {
    private readonly ILogger<OpenAiApiClient> _logger;
    private readonly OpenAIAPI _api;
    private readonly ConcurrentDictionary<Guid, List<ChatMessage>> _conversations;

    public OpenAiApiClient(string apiKey, ILogger<OpenAiApiClient>? logger = null) {
        _logger = logger ?? NullLogger<OpenAiApiClient>.Instance;
        _api = new OpenAIAPI(apiKey);
        _conversations = new ConcurrentDictionary<Guid, List<ChatMessage>>();
    }

    public async Task<Model[]> GetModelsAsync() {
        try {
            var models = await _api.Models.GetModelsAsync().ConfigureAwait(false);
            return models.ToArray();

        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to get models.");
            throw;
        }
    }

    public async Task<Model?> GetModelByIdAsync(string id) {
        try {
            return await _api.Models.RetrieveModelDetailsAsync(id).ConfigureAwait(false);

        }
        catch (Exception ex) {
            if (ex.Message.Contains("HTTP status code: NotFound"))
                return null;
            _logger.LogError(ex, "Failed to get models.");
            throw;
        }
    }

    public async Task<CreateChatResult> StartChatAsync(string? system = null) {
        try {
            var chatId = Guid.NewGuid();
            system = string.IsNullOrWhiteSpace(system) ? "You are a general assistant to the user." : system;
            _conversations.TryAdd(chatId, new List<ChatMessage> { new(ChatMessageRole.System, system) });

            return await GetChatResponseAsync(chatId).ConfigureAwait(false);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to start a new chat.");
            throw;
        }
    }

    public async Task<CreateChatResult?> SendPromptAsync(Guid chatId, string prompt) {
        if (!_conversations.ContainsKey(chatId))
            return null;

        try {
            _conversations[chatId].Add(new ChatMessage(ChatMessageRole.User, prompt));
            return await GetChatResponseAsync(chatId).ConfigureAwait(false);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to send prompt.");
            throw;
        }
    }

    private async Task<CreateChatResult> GetChatResponseAsync(Guid chatId) {
        var request = CreateChatRequest(chatId);
        var chat = await _api.Chat.CreateChatCompletionAsync(request).ConfigureAwait(false);
        var reply = chat.Choices[0].Message;
        _conversations[chatId].Add(reply);
        return CreateChatResult(chatId, reply);
    }

    private ChatRequest CreateChatRequest(Guid chatId)
        => new() {
            Model = Model.GPT4_32k_Context,
            Temperature = 1.0,
            MaxTokens = 8000,
            Messages = _conversations[chatId],
        };

    private static CreateChatResult CreateChatResult(Guid chatId, ChatMessage reply)
    {
        var role = reply.Role.ToString();
        var message = reply.Content.Trim();
        return new CreateChatResult(chatId, role, message);
    }
}