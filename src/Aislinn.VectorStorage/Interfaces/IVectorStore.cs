using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.VectorStorage.Models;

namespace Aislinn.VectorStorage.Interfaces
{
    //unused, since we just have the base service for now that uses in-memory storage
    public interface IVectorStore
    {
        Task AddVectorAsync(string chunkId, string text, Dictionary<string, string> metadata);
        Task<List<SearchResult>> SearchVectorsAsync(string query, int topN, double minSimilarity = 0.0);
        Task<List<SearchResult>> SearchVectorsAsync(double[] queryVector, int topN, double minSimilarity = 0.0);
        Task<bool> DeleteVectorAsync(string chunkId);
        Task<Dictionary<string, string>> GetMetadataAsync(string chunkId);
        Task<int> GetVectorCountAsync();
        Task<VectorItem> GetVectorAsync(string chunkId);
    }
}