namespace Pluto.Application.DTOs.MessageEmbeddings;

public record MessageEmbeddingsResponse(IEnumerable<MessageBody>? Messages);