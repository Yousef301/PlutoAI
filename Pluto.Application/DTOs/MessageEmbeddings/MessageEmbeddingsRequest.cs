namespace Pluto.Application.DTOs.MessageEmbeddings;

public record MessageEmbeddingsRequest(
    IEnumerable<MessageBody> Messages,
    string query,
    int Limit
);