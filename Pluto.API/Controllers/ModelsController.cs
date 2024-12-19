using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/models")]
public class ModelsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public ModelsController(
        IConfiguration config,
        IHttpClientFactory httpClientFactory
    )
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetModels()
    {
        var client = _httpClientFactory.CreateClient();

        try
        {
            var endpoint = _config["ModelsEndpoint"];

            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch models.");
            }

            var responseData = await response.Content.ReadAsStringAsync();

            var dataAsJson = JsonSerializer.Deserialize<object>(responseData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return Ok(dataAsJson);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error fetching models: {ex.Message}");
        }
    }
}