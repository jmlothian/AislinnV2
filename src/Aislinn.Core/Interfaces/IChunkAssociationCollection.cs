// Interface for ChunkAssociation collection
using Aislinn.Core.Models;
namespace Aislinn.ChunkStorage.Interfaces;

public interface IChunkAssociationCollection
{
    Task<ChunkAssociation> AddAssociationAsync(ChunkAssociation association);
    Task<ChunkAssociation> GetAssociationAsync(Guid chunkAId, Guid chunkBId, string relationAtoB, string relationBtoA);
    Task<bool> UpdateAssociationAsync(ChunkAssociation association);
    Task<bool> DeleteAssociationAsync(Guid chunkAId, Guid chunkBId, string relationAtoB, string relationBtoA);
    Task<List<ChunkAssociation>> GetAssociationsForChunkAsync(Guid chunkId);
    Task<List<ChunkAssociation>> GetAllAssociationsAsync(); //for decay
    Task<List<ChunkAssociation>> GetAllAssociationsBetweenChunksAsync(Guid chunkAId, Guid chunkBId);
    Task<bool> HasAssociationAsync(Guid chunkAId, Guid chunkBId);

}