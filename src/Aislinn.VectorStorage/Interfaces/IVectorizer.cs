using System.Threading.Tasks;

namespace Aislinn.VectorStorage.Interfaces
{
    public interface IVectorizer
    {
        Task<double[]> StringToVectorAsync(string text);
        int Dimensions { get; }
    }
}