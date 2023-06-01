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
            TotalTokens += result.Usage.TotalTokens;
            var res = result.Data[0].Embedding.ToArray<double>();
            return res;
        }
    }
}
