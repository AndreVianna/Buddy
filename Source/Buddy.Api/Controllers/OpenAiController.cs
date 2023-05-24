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

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] string? system = null)
    {
        try
        {
            var chat = await _client.CreateAsync(system);
            return Ok(chat);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> SendAsync([FromRoute] Guid id, [FromBody] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return BadRequest("Prompt cannot be empty.");
        }

        try
        {
            var answer = await _client.SendAsync(id, prompt);
            if (answer is null)
            {
                return NotFound();
            }

            return Ok(answer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}