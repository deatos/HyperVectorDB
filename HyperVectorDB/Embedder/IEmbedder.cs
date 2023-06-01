using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB.Embedder {
    public interface IEmbedder {

        public Double[] GetVector(String Document);
        public Double[][] GetVectors(String[] Documents);

    }
}
