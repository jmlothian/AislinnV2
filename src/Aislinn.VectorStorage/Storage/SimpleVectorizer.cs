using System;
using System.Threading.Tasks;
using Aislinn.VectorStorage.Interfaces;

namespace Aislinn.VectorStorage.Storage
{
    /// <summary>
    /// A simple implementation of IVectorizer that creates vectors based on character frequencies
    /// For a real implementation, you would use a proper NLP embedding model
    /// </summary>
    public class SimpleVectorizer : IVectorizer
    {
        private readonly Random _random;

        /// <summary>
        /// Gets the dimension of vectors created by this vectorizer
        /// </summary>
        public int Dimensions { get; }

        public SimpleVectorizer(int dimensions = 128)
        {
            Dimensions = dimensions;
            _random = new Random(42); // Fixed seed for reproducibility
        }

        /// <summary>
        /// Converts a string to a vector representation asynchronously
        /// </summary>
        public async Task<double[]> StringToVectorAsync(string text)
        {
            // Simulate some async work
            await Task.Delay(1);
            
            if (string.IsNullOrEmpty(text))
                return new double[Dimensions]; // Return zero vector

            // Create a simple character frequency-based vector
            // This is not a proper embedding but serves as a placeholder
            var vector = new double[Dimensions];
            
            // Normalize the text
            text = text.ToLowerInvariant();
            
            // Fill the first part of the vector with character frequencies
            for (int i = 0; i < Math.Min(128, text.Length); i++)
            {
                int charIndex = text[i] % Dimensions;
                vector[charIndex] += 1.0 / text.Length;
            }
            
            // Apply some basic normalization
            double sum = 0;
            for (int i = 0; i < Dimensions; i++)
            {
                sum += vector[i] * vector[i];
            }
            
            if (sum > 0)
            {
                double norm = Math.Sqrt(sum);
                for (int i = 0; i < Dimensions; i++)
                {
                    vector[i] /= norm;
                }
            }
            
            return vector;
        }
    }
}