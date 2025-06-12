using System.Threading.Tasks;

namespace Aislinn.VectorStorage.Interfaces
{
    public interface IVectorizer
    {
        Task<double[]> StringToVectorAsync(string text);
        Task<double[]> StringToVectorAsync(string text, string inputType); // Add overload

        int Dimensions { get; }
    }
}