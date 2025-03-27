using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.Core.Models;

namespace Aislinn.ChunkStorage.Interfaces
{
    public interface IChunkCollection
    {
        string Id { get; }

        Task<Chunk> AddChunkAsync(Chunk chunk);
        Task<Chunk> GetChunkAsync(Guid chunkId);
        Task<bool> UpdateChunkAsync(Chunk chunk);
        Task<bool> DeleteChunkAsync(Guid chunkId);
        Task<int> GetChunkCountAsync();
        Task<List<Chunk>> GetAllChunksAsync();
    }

}