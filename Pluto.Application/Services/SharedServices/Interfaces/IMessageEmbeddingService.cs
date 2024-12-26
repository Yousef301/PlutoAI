using Pluto.Application.DTOs.MessageEmbeddings;

namespace Pluto.Application.Services.SharedServices.Interfaces;

public interface IMessageEmbeddingService
{
    Task<IEnumerable<MessageBody>?> GetSimilarMessages(MessageEmbeddingsRequest message);
}