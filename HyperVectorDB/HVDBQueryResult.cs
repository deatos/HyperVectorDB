using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB {
    public class HVDBQueryResult {
        public List<HVDBDocument> Documents { get; set; }
        public List<double> Distances { get; set; }

        public HVDBQueryResult(List<HVDBDocument> documents, List<double> distances) {
            Documents = documents;
            Distances = distances;
        }

    }
}
