using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.Core.Models;

namespace Aislinn.ChunkStorage.Interfaces
{


    public interface IChunkStore
    {
        Task<IChunkCollection> CreateCollectionAsync(string collectionId);
        Task<IChunkCollection> GetCollectionAsync(string collectionId);
        Task<IChunkCollection> GetOrCreateCollectionAsync(string collectionId);
        Task<bool> DeleteCollectionAsync(string collectionId);
        Task<List<string>> GetCollectionIdsAsync();
    }
}