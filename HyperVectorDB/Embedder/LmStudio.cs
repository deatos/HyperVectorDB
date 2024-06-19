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
    public class LmStudio : IEmbedder
    {
        public LmStudio()
        {

        }
        public Double[] GetVector(String Document)
        {
            EmbeddingRequest er = new()
            {
                input = Document,
                model = "CompendiumLabs/bge-large-en-v1.5-gguf"
            };


            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:1234/v1/embeddings");
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
        public Double[][] GetVectors(String[] Documents)
        {
            throw new NotImplementedException();
            double[][] vmatrix = new double[1][];
            vmatrix[0] = new double[1];
            return vmatrix;
        }
    }


    class EmbeddingRequest
    {
        public string input { get; set; }
        public string model { get; set; }
    }
    // EmbeddingResponse response = JsonConvert.DeserializeObject<EmbeddingResponse>(myJsonResponse);
    class Datum
    {
        public string @object { get; set; }
        public List<double> embedding { get; set; }
        public int index { get; set; }
    }

    class EmbeddingResponse
    {
        public string @object { get; set; }
        public List<Datum> data { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
    }

    class Usage
    {
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
    }

}