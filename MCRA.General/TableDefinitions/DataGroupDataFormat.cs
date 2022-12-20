using System.Collections.Generic;
using System.Xml.Serialization;

namespace MCRA.General.TableDefinitions {
    public class DataGroupDataFormat {

        /// <summary>
        /// A string containing a unique identified for the table.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A string containing a unique identified for the table.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A string containing a unique identified for the table.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of table IDs of the tables that are part of this data format.
        /// </summary>
        [XmlArrayItem("TableId")]
        public List<string> TableIds { get; set; }

    }
}
