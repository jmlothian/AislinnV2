using System.Collections.Generic;
using System.Threading.Tasks;
using Aislinn.VectorStorage.Models;

namespace Aislinn.VectorStorage.Interfaces
{
    public interface IVectorCollection
    {
        string Id { get; }
        Task<VectorItem> AddVectorAsync(string text, Dictionary<string, string> metadata);
        Task<VectorItem> AddVectorAsync(string vectorId, string text, Dictionary<string, string> metadata);
        Task<List<SearchResult>> SearchVectorsAsync(string query, int topN, double minSimilarity = 0.0);
        Task<List<SearchResult>> SearchVectorsAsync(double[] queryVector, int topN, double minSimilarity = 0.0);
        Task<bool> DeleteVectorAsync(string vectorId);
        Task<Dictionary<string, string>> GetMetadataAsync(string vectorId);
        Task<int> GetVectorCountAsync();
        Task<VectorItem> GetVectorAsync(string vectorId);
    }
}