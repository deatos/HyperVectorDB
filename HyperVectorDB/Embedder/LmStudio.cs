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
        }
    }


    class EmbeddingRequest
    {
        required public string input { get; set; }
        required public string model { get; set; }
    }

    class Datum
    {
        required public string @object { get; set; }
        required public List<double> embedding { get; set; }
        required public int index { get; set; }
    }

    class EmbeddingResponse
    {
        required public string @object { get; set; }
        required public List<Datum> data { get; set; }
        required public string model { get; set; }
        required public Usage usage { get; set; }
    }

    class Usage
    {
        public int prompt_tokens { get; set; }
        public int total_tokens { get; set; }
    }

}