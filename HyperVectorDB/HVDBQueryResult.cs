using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB {
    /// <summary>
    /// Container class for `HVDBDocument` records and their linear distance from the query string.
    /// </summary>
    public class HVDBQueryResult {

        /// <summary>
        /// Closest `HVDBDocument` records found in the database
        /// </summary>
        public List<HVDBDocument> Documents { get; set; }

        /// <summary>
        /// Distances of each `HVDBDocument` record from the original prompt
        /// </summary>
        public List<double> Distances { get; set; }

        /// <summary>
        /// Full constructor for packing the document records and distances
        /// </summary>
        /// <param name="documents">Closest `HVDBDocument` records found in the database</param>
        /// <param name="distances">Distances of each `HVDBDocument` record from the original prompt</param>
        public HVDBQueryResult(List<HVDBDocument> documents, List<double> distances) {
            Documents = documents;
            Distances = distances;
        }

    }
}
