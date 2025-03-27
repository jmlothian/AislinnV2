using System.Collections.Generic;

namespace Aislinn.VectorStorage.Models
{
    
    public class SearchResult
    {
        public double Similarity { get; set; }
        public VectorItem Value { get; set; }
        public string Collection {get; set;}

        public SearchResult(VectorItem vectorItem, double similarity, string collection)
        {
            Similarity = similarity;
            Value = vectorItem;
            Collection = collection;
        }
    }
}