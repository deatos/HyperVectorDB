using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB.Embedder {
	/// <summary>
	/// Implementation of IEmbedder that uses OpenAI's embedding models to convert text into vector representations.
	/// </summary>
	public class EmbedderOpenAI : IEmbedder {
		private readonly OpenAI.OpenAIClient Client;
		/// <summary>
		/// Tracks the total number of tokens used across all embedding requests.
		/// </summary>
		public static int TotalTokens = 0;
		private OpenAI.Models.Model _oAIModel = OpenAI.Models.Model.Embedding_3_Small; //default to cheapest model

		/// <summary>
		/// Initializes a new instance of the EmbedderOpenAI class.
		/// </summary>
		/// <param name="OpenAIApiKey">The API key for accessing OpenAI's services.</param>
		/// <param name="embeddingModel">The OpenAI embedding model to use for vector generation.</param>
		public EmbedderOpenAI(string OpenAIApiKey, OAIEmbeddingModel embeddingModel) {
			this.Client = new OpenAI.OpenAIClient(new OpenAI.OpenAIAuthentication(OpenAIApiKey));
			if (embeddingModel == OAIEmbeddingModel.text_embedding_3_small) _oAIModel = OpenAI.Models.Model.Embedding_3_Small;
			if(embeddingModel == OAIEmbeddingModel.text_embedding_3_large) _oAIModel = OpenAI.Models.Model.Embedding_3_Large;
			if(embeddingModel == OAIEmbeddingModel.ada_v2) _oAIModel = OpenAI.Models.Model.Embedding_Ada_002;
		}

		/// <inheritdoc/>
		public double[] GetVector(string Document) {
			var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Document, _oAIModel).GetAwaiter().GetResult();
			TotalTokens += result.Usage.TotalTokens ?? 0; //TODO: Check if this is correct and why openai made this nullable
			var vect = result.Data[0].Embedding.ToArray<double>();
			return vect;
		}

		/// <inheritdoc/>
		public double[][] GetVectors(string[] Documents) {
			var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Documents, OpenAI.Models.Model.Embedding_Ada_002).GetAwaiter().GetResult();
			TotalTokens += result.Usage.TotalTokens ?? 0; //TODO: Check if this is correct and why openai made this nullable
			var vmatrix = result.Data.Select(x => x.Embedding.ToArray<double>()).ToArray();
			return vmatrix;
		}

		/// <summary>
		/// Enumeration of available OpenAI embedding models.
		/// </summary>
		public enum OAIEmbeddingModel {
			/// <summary>
			/// OpenAI's text-embedding-3-small model
			/// </summary>
			text_embedding_3_small,
			/// <summary>
			/// OpenAI's text-embedding-3-large model
			/// </summary>
			text_embedding_3_large,
			/// <summary>
			/// OpenAI's text-embedding-ada-002 model
			/// </summary>
			ada_v2
		}
	}
}
