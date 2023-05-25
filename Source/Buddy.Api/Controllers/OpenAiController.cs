using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Buddy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenAiController : ControllerBase
{
    private readonly OpenAiApiClient _client;

    public OpenAiController(OpenAiApiClient client)
    {
        _client = client;
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModelsAsync() {
        try {
            var models = await _client.GetModelsAsync();
            return Ok(models);
        }
        catch (Exception ex) {
            return StatusCode(Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("models/{id}")]
    public async Task<IActionResult> GetModelByIdAsync([FromRoute] string id) {
        try {
            var model = await _client.GetModelByIdAsync(id);
            if (model is null) return NotFound($"Model '{id}' not found.");
            return Ok(model);
        }
        catch (Exception ex) {
            return StatusCode(Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> CreateAsync([FromBody] string? system = null)
    {
        try
        {
            var chat = await _client.StartChatAsync(system);
            return Ok(chat);
        }
        catch (Exception ex)
        {
            return StatusCode(Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost("chat/{id:guid}")]
    public async Task<IActionResult> SendAsync([FromRoute] Guid id, [FromBody] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return BadRequest("Prompt cannot be empty.");
        }

        try
        {
            var answer = await _client.SendPromptAsync(id, prompt);
            if (answer is null)
            {
                return NotFound();
            }

            return Ok(answer);
        }
        catch (Exception ex)
        {
            return StatusCode(Status500InternalServerError, ex.Message);
        }
    }
}