using System.Threading.Tasks;

namespace Aislinn.VectorStorage.Interfaces
{
    public interface IVectorizer
    {
        Task<double[]> StringToVectorAsync(string text);
        Task<double[]> StringToVectorAsync(string text, string inputType); // Add overload
        //batch methods
        Task<List<double[]>> StringsToVectorsAsync(IEnumerable<string> texts);
        Task<List<double[]>> StringsToVectorsAsync(IEnumerable<string> texts, string inputType = null);
        int Dimensions { get; }
    }
}