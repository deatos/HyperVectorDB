using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperVectorDB {
    public class HVDBDocument {
        public string ID { get; set; }
        public string Name { get; set; }
        public string DocumentString { get; set; }

        public HVDBDocument(string name, string documentstring) {
            ID = Guid.NewGuid().ToString();
            Name = name;
            DocumentString = documentstring;
        }
        public HVDBDocument(string id, string name, string documentstring) {
            ID = id;
            Name = name;
            DocumentString = documentstring;
        }

    }
}
