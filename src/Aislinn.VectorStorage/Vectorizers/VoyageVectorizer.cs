using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Aislinn.VectorStorage.Interfaces;

namespace Aislinn.VectorStorage.Implementations
{

    public class VoyageVectorizer : IVectorizer, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly VoyageConfiguration _config;
        private readonly int _dimensions;

        public int Dimensions => _dimensions;

        public VoyageVectorizer(VoyageConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrEmpty(_config.VoyageApiKey))
                throw new ArgumentException("VoyageApiKey is required", nameof(config));

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_config.VoyageTimeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.VoyageApiKey}");

            // Set dimensions based on model
            _dimensions = GetModelDimensions(_config.VoyageModel);
        }

        public async Task<double[]> StringToVectorAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            var request = new VoyageEmbeddingRequest
            {
                input = new[] { text },
                model = _config.VoyageModel,
                input_type = _config.VoyageInputType
            };

            (bool flowControl, double[] value) = await RequestVector(request);
            if (!flowControl)
            {
                return value;
            }

            throw new InvalidOperationException($"Failed to get embedding after {_config.VoyageMaxRetries + 1} attempts");
        }

        private async Task<(bool flowControl, double[] value)> RequestVector(VoyageEmbeddingRequest request)
        {
            var retryCount = 0;
            while (retryCount <= _config.VoyageMaxRetries)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(_config.VoyageBaseUrl, request);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<VoyageEmbeddingResponse>(responseContent);

                        if (result?.data?.Length > 0)
                        {
                            return (flowControl: false, value: result.data[0].embedding);
                        }

                        throw new InvalidOperationException("No embedding data returned from Voyage API");
                    }

                    // Handle rate limiting
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                        await Task.Delay(retryAfter);
                        retryCount++;
                        continue;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Voyage API error: {response.StatusCode} - {errorContent}");
                }
                catch (TaskCanceledException) when (retryCount < _config.VoyageMaxRetries)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                }
                catch (HttpRequestException) when (retryCount < _config.VoyageMaxRetries)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                }
            }

            return (flowControl: true, value: null);
        }
        private async Task<(bool flowControl, List<double[]> values)> RequestVectors(VoyageEmbeddingRequest request)
        {
            var retryCount = 0;
            while (retryCount <= _config.VoyageMaxRetries)
            {
                try
                {
                    // Use the batch endpoint for multiple inputs
                    var response = await _httpClient.PostAsJsonAsync(_config.VoyageBaseUrl, request);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<VoyageEmbeddingResponse>(responseContent);

                        if (result?.data?.Length > 0)
                        {
                            var vectors = result.data
                                .OrderBy(d => d.index) // Ensure correct order matches input order
                                .Select(d => d.embedding)
                                .ToList();
                            return (flowControl: false, values: vectors);
                        }

                        throw new InvalidOperationException("No embedding data returned from Voyage API");
                    }

                    // Handle rate limiting
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                        await Task.Delay(retryAfter);
                        retryCount++;
                        continue;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Voyage API error: {response.StatusCode} - {errorContent}");
                }
                catch (TaskCanceledException) when (retryCount < _config.VoyageMaxRetries)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                }
                catch (HttpRequestException) when (retryCount < _config.VoyageMaxRetries)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                }
            }

            return (flowControl: true, values: null);
        }

        public async Task<List<double[]>> StringsToVectorsAsync(IEnumerable<string> texts)
        {
            return await StringsToVectorsAsync(texts, null);
        }

        public async Task<List<double[]>> StringsToVectorsAsync(IEnumerable<string> texts, string inputType = null)
        {
            if (texts == null)
                throw new ArgumentNullException(nameof(texts));

            var textArray = texts.ToArray();
            if (textArray.Length == 0)
                return new List<double[]>();

            var request = new VoyageEmbeddingRequest
            {
                input = textArray,
                model = _config.VoyageModel,
                input_type = inputType ?? _config.VoyageInputType
            };

            (bool flowControl, List<double[]> values) = await RequestVectors(request);
            if (!flowControl)
            {
                return values;
            }

            throw new InvalidOperationException($"Failed to get embeddings after {_config.VoyageMaxRetries + 1} attempts");
        }
        private static int GetModelDimensions(string model)
        {
            return model?.ToLower() switch
            {
                "voyage-3-large" => 1024,
                "voyage-3.5" => 1024,
                "voyage-3.5-lite" => 512,
                "voyage-code-3" => 1024,
                "voyage-finance-2" => 1024,
                "voyage-law-2" => 1024,
                "voyage-multilingual-2" => 1024,
                "voyage-2" => 1024,
                "voyage-code-2" => 1536,
                "voyage-large-2" => 1536,
                "voyage-large-2-instruct" => 1536,
                _ => 1024 // Default fallback
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public async Task<double[]> StringToVectorAsync(string text, string inputType = null)
        {
            var request = new VoyageEmbeddingRequest
            {
                input = new[] { text },
                model = _config.VoyageModel,
                input_type = inputType ?? _config.VoyageInputType // Use parameter or fallback to config
            };
            (bool flowControl, double[] value) = await RequestVector(request);
            if (!flowControl)
            {
                return value;
            }
            return null;

        }
    }

    // Internal DTOs for Voyage API
    internal class VoyageEmbeddingRequest
    {
        public string[] input { get; set; }
        public string model { get; set; }
        public string input_type { get; set; }
    }

    internal class VoyageEmbeddingResponse
    {
        public VoyageEmbeddingData[] data { get; set; }
        public VoyageUsage usage { get; set; }
    }

    internal class VoyageEmbeddingData
    {
        public double[] embedding { get; set; }
        public int index { get; set; }

    }

    internal class VoyageUsage
    {
        public int total_tokens { get; set; }
    }
}