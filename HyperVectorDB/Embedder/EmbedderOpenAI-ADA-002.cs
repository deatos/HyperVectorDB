using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB.Embedder {
    public class EmbedderOpenAI_ADA_002 : IEmbedder {
        private readonly OpenAI.OpenAIClient Client;
        public static int TotalTokens = 0;
        public EmbedderOpenAI_ADA_002(string openaiapikey) {
            this.Client = new OpenAI.OpenAIClient(new OpenAI.OpenAIAuthentication(openaiapikey));
        }
        public Double[] GetVector(String Document) {
            var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Document, OpenAI.Models.Model.Embedding_Ada_002).GetAwaiter().GetResult();
            TotalTokens += result.Usage.TotalTokens ?? 0; //TODO: Check if this is correct and why openai made this nullable
			var vect = result.Data[0].Embedding.ToArray<double>();
            return vect;
        }
        public Double[][] GetVectors(String[] Documents) {
            var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Documents, OpenAI.Models.Model.Embedding_Ada_002).GetAwaiter().GetResult();
            TotalTokens += result.Usage.TotalTokens ?? 0; //TODO: Check if this is correct and why openai made this nullable
            var vmatrix = result.Data.Select(x => x.Embedding.ToArray<double>()).ToArray();
            return vmatrix;
        }
    }
}