// Interface for ChunkAssociation collection
using Aislinn.Core.Models;
namespace Aislinn.ChunkStorage.Interfaces;
public interface IChunkAssociationCollection
{
    Task<ChunkAssociation> AddAssociationAsync(ChunkAssociation association);
    Task<ChunkAssociation> GetAssociationAsync(Guid chunkAId, Guid chunkBId);
    Task<bool> UpdateAssociationAsync(ChunkAssociation association);
    Task<bool> DeleteAssociationAsync(Guid chunkAId, Guid chunkBId);
    Task<List<ChunkAssociation>> GetAssociationsForChunkAsync(Guid chunkId);
}