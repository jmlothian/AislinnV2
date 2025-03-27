using System.Collections.Generic;

namespace Aislinn.VectorStorage.Models
{
    public class VectorItem
    {
        public string ID { get; set; }
        public double[] Vector { get; set; }
        public string Text { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public VectorItem(string id, string text, double[] vector, Dictionary<string, string> metadata)
        {
            Text = text;
            Vector = vector;
            Metadata = metadata ?? new Dictionary<string, string>();
            ID = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }
        public VectorItem Clone()
        {
            // Deep copy the vector array
            double[] vectorCopy = Vector != null ? (double[])Vector.Clone() : null;

            // Deep copy the metadata dictionary
            Dictionary<string, string> metadataCopy = null;
            if (Metadata != null)
            {
                metadataCopy = new Dictionary<string, string>(Metadata);
            }

            // Create new VectorItem with copied components
            return new VectorItem(ID, Text, vectorCopy, metadataCopy);
        }
    }
}