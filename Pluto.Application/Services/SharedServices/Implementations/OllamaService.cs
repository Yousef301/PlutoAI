using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Exceptions;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.SharedServices.Implementations;

public class OllamaService : IModelService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public OllamaService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory
    )
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }


    public async Task<string> GenerateResponseAsync(string prompt)
    {
        var ollamaUrl = _configuration["Ollama:Url"];
        var model = _configuration["Ollama:Model"];

        if (string.IsNullOrEmpty(ollamaUrl) || string.IsNullOrEmpty(model))
            throw new InvalidConfigurationException("Ollama API configuration is missing.");

        var requestPayload = new
        {
            model = model,
            prompt = prompt,
            stream = false
        };

        var jsonPayload = JsonSerializer.Serialize(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.PostAsync(ollamaUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OllamaResponse>(responseString);

                if (responseObject == null || string.IsNullOrEmpty(responseObject.Response))
                {
                    throw new InvalidOperationException("Invalid response from Ollama API: Response is null or empty.");
                }

                return responseObject.Response;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Ollama API Error: {response.StatusCode}, Details: {errorContent}");
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while generating a response from Ollama.", ex);
        }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")] public string Response { get; set; }
    }
}