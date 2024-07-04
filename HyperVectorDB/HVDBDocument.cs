using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace HyperVectorDB {
    [MessagePackObject]
    public class HVDBDocument {
        [Key(0)]
        public string ID { get; set; }
        [Key(1)]
        public string DocumentString { get; set; }
        public HVDBDocument() {
            ID = Guid.NewGuid().ToString();
            DocumentString = string.Empty;
        }
        public HVDBDocument(string documentstring) {
            ID = Guid.NewGuid().ToString();
            DocumentString = documentstring;
        }
        public HVDBDocument(string id, string documentstring) {
            ID = id;
            DocumentString = documentstring;
        }

    }
}
