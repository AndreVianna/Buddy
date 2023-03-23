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

    [HttpGet("send")]
    public async Task<IActionResult> GenerateChatAsync(string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            return BadRequest("Prompt cannot be empty.");
        }

        try
        {
            var response = await _client.GenerateTextAsync(prompt);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
