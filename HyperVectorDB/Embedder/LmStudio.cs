using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HyperVectorDB.Embedder
{
    /// <summary>
    /// Implementation of IEmbedder that uses LM Studio's local embedding service to convert text into vector representations.
    /// </summary>
    public class LmStudio : IEmbedder
    {
        /// <summary>
        /// Gets or sets the URL of the LM Studio embedding service.
        /// </summary>
        public string URL { get; set; } = @"http://localhost:1234/v1/embeddings";

        /// <summary>
        /// Gets or sets the model name to use for embeddings.
        /// </summary>
        public string Model { get; set; } = @"CompendiumLabs/bge-large-en-v1.5-gguf";

        /// <inheritdoc/>
        public double[] GetVector(string Document)
        {
            EmbeddingRequest er = new()
            {
                input = Document,
                model = Model
            };

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, URL);
            request.Content = new StringContent(JsonSerializer.Serialize(er));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage httpResponse = client.Send(request);
            httpResponse.EnsureSuccessStatusCode();
            var responseBody = httpResponse.Content.ReadAsStringAsync();
            responseBody.Wait();
            string responseJson = responseBody.Result;

            EmbeddingResponse response = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson)!;

            double[] vect = response.data.First().embedding.ToArray();
            return vect;
        }

        /// <inheritdoc/>
        public double[][] GetVectors(string[] Documents)
        {
            List<double[]> vectors = new();
            foreach (string document in Documents)
            {
                vectors.Add(GetVector(document));
            }
            return vectors.ToArray();
        }
    }

    /// <summary>
    /// Represents a request to the LM Studio embedding service.
    /// </summary>
    class EmbeddingRequest
    {
        /// <summary>
        /// The input text to be embedded.
        /// </summary>
        required public string input { get; set; }

        /// <summary>
        /// The model to use for generating embeddings.
        /// </summary>
        required public string model { get; set; }
    }

    /// <summary>
    /// Represents a single embedding datum in the response.
    /// </summary>
    class Datum
    {
        /// <summary>
        /// The object type, typically "embedding".
        /// </summary>
        required public string @object { get; set; }

        /// <summary>
        /// The embedding vector.
        /// </summary>
        required public List<double> embedding { get; set; }

        /// <summary>
        /// The index of this embedding in the response.
        /// </summary>
        required public int index { get; set; }
    }

    /// <summary>
    /// Represents the response from the LM Studio embedding service.
    /// </summary>
    class EmbeddingResponse
    {
        /// <summary>
        /// The object type, typically "list".
        /// </summary>
        required public string @object { get; set; }

        /// <summary>
        /// The list of embedding data.
        /// </summary>
        required public List<Datum> data { get; set; }

        /// <summary>
        /// The model used to generate the embeddings.
        /// </summary>
        required public string model { get; set; }

        /// <summary>
        /// Usage statistics for the request.
        /// </summary>
        required public Usage usage { get; set; }
    }

    /// <summary>
    /// Represents usage statistics for an embedding request.
    /// </summary>
    class Usage
    {
        /// <summary>
        /// The number of tokens in the prompt.
        /// </summary>
        public int prompt_tokens { get; set; }

        /// <summary>
        /// The total number of tokens used.
        /// </summary>
        public int total_tokens { get; set; }
    }
}