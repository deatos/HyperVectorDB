using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB.Embedder {
    /// <summary>
    /// Interface for text embedding providers that convert text into vector representations.
    /// </summary>
    public interface IEmbedder {
        /// <summary>
        /// Converts a single document into a vector representation.
        /// </summary>
        /// <param name="Document">The text document to convert into a vector.</param>
        /// <returns>A vector representation of the document.</returns>
        public Double[] GetVector(String Document);

        /// <summary>
        /// Converts multiple documents into vector representations.
        /// </summary>
        /// <param name="Documents">An array of text documents to convert into vectors.</param>
        /// <returns>An array of vector representations for each document.</returns>
        public Double[][] GetVectors(String[] Documents);
    }
}
