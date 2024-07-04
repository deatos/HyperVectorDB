using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace HyperVectorDB {
    /// <summary>
    /// Basic object associating a text sequence with a globally unique ID.
    /// </summary>
    [MessagePackObject]
    public class HVDBDocument {
        /// <summary>
        /// Unique ID of this document. Must be unique within the database. Will be a [GUID](https://learn.microsoft.com/en-us/dotnet/api/system.guid?view=net-8.0) if not specified.
        /// </summary>
        [Key(0)]
        public string ID { get; set; }

        /// <summary>
        /// Text stored within this document.
        /// </summary>
        [Key(1)]
        public string DocumentString { get; set; }

        /// <summary>
        /// Default constructor for reflection purposes. Automatically generates a GUID for the `ID` property.
        /// </summary>
        public HVDBDocument() {
            ID = Guid.NewGuid().ToString();
            DocumentString = string.Empty;
        }

        /// <summary>
        /// Basic constructor. Automatically generates a GUID for the `ID` property.
        /// </summary>
        /// <param name="documentstring">Text to be stored within this document</param>
        public HVDBDocument(string documentstring) {
            ID = Guid.NewGuid().ToString();
            DocumentString = documentstring;
        }

        /// <summary>
        /// Full constructor for serialization purposes.
        /// </summary>
        /// <param name="id">Unique ID for this document. This must be unique within the database.</param>
        /// <param name="documentstring">Text to be stored within this document</param>
        public HVDBDocument(string id, string documentstring) {
            ID = id;
            DocumentString = documentstring;
        }

    }
}
