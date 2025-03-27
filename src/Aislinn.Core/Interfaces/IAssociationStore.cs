
namespace Aislinn.ChunkStorage.Interfaces;
// Interface for Association Store
public interface IAssociationStore
{
    Task<IChunkAssociationCollection> CreateCollectionAsync(string collectionId);
    Task<IChunkAssociationCollection> GetCollectionAsync(string collectionId);
    Task<IChunkAssociationCollection> GetOrCreateCollectionAsync(string collectionId);
    Task<bool> DeleteCollectionAsync(string collectionId);
    Task<List<string>> GetCollectionIdsAsync();
}