using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB.Embedder {
	public class EmbedderOpenAI : IEmbedder {
		private readonly OpenAI.OpenAIClient Client;
		public static int TotalTokens = 0;
		private OpenAI.Models.Model _oAIModel = OpenAI.Models.Model.Embedding_3_Small; //default to cheapest model

		public EmbedderOpenAI(string OpenAIApiKey, OAIEmbeddingModel embeddingModel) {
			this.Client = new OpenAI.OpenAIClient(new OpenAI.OpenAIAuthentication(OpenAIApiKey));
			if (embeddingModel == OAIEmbeddingModel.text_embedding_3_small) _oAIModel = OpenAI.Models.Model.Embedding_3_Small;
			if(embeddingModel == OAIEmbeddingModel.text_embedding_3_large) _oAIModel = OpenAI.Models.Model.Embedding_3_Large;
			if(embeddingModel == OAIEmbeddingModel.ada_v2) _oAIModel = OpenAI.Models.Model.Embedding_Ada_002;
		}
		public double[] GetVector(string Document) {
			var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Document, _oAIModel).GetAwaiter().GetResult();
			TotalTokens += result.Usage.TotalTokens ?? 0; //TODO: Check if this is correct and why openai made this nullable
			var vect = result.Data[0].Embedding.ToArray<double>();
			return vect;
		}

		public double[][] GetVectors(string[] Documents) {
			var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Documents, OpenAI.Models.Model.Embedding_Ada_002).GetAwaiter().GetResult();
			TotalTokens += result.Usage.TotalTokens ?? 0; //TODO: Check if this is correct and why openai made this nullable
			var vmatrix = result.Data.Select(x => x.Embedding.ToArray<double>()).ToArray();
			return vmatrix;
		}

		public enum OAIEmbeddingModel {
			text_embedding_3_small,
			text_embedding_3_large,
			ada_v2
		}
	}
}
