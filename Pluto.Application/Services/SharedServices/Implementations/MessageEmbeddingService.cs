using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.MessageEmbeddings;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Exceptions;

namespace Pluto.Application.Services.SharedServices.Implementations;

public class MessageEmbeddingService : IMessageEmbeddingService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public MessageEmbeddingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    )
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IEnumerable<MessageBody>?> GetSimilarMessages(MessageEmbeddingsRequest request)
    {
        var similarUrl = _configuration["MessageEmbeddings:SimilarUrl"];

        if (string.IsNullOrEmpty(similarUrl))
            throw new InvalidConfigurationException("MessageEmbedding API configuration is missing.");

        var requestPayload = new
        {
            messages = request.Messages,
            query = request.query,
            top_k = request.Limit
        };

        var jsonPayload = JsonSerializer.Serialize(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var client = _httpClientFactory.CreateClient();

        var response = await client.PostAsync(similarUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var messages = JsonSerializer.Deserialize<IEnumerable<MessageBody>>(responseString, options);
                return messages ?? Enumerable.Empty<MessageBody>();
            }
            catch
            {
                try
                {
                    var wrapper = JsonSerializer.Deserialize<MessageEmbeddingsResponse>(responseString, options);
                    return wrapper?.Messages ?? Enumerable.Empty<MessageBody>();
                }
                catch
                {
                    return Enumerable.Empty<MessageBody>();
                }
            }
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Message Embeddings API Error: {response.StatusCode}, Details: {errorContent}");
    }
}