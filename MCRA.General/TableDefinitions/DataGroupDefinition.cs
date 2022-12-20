using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MCRA.General.TableDefinitions {

    public class DataGroupDefinition {

        /// <summary>
        /// A string containing a unique identified for the table.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A string containing the name of the table.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the data defined by this table definition.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of data formats. If null or empty, then the data group itself
        /// is considered the one and only data format.
        /// </summary>
        [XmlArrayItem("DataFormat")]
        public List<DataGroupDataFormat> DataFormats { get; set; }

        /// <summary>
        /// Additional technical notes.
        /// </summary>
        [XmlArrayItem("TechnicalNote")]
        public List<string> TechnicalNotes { get; set; }

        /// <summary>
        /// A string of the name of the table group to which this definition belongs.
        /// </summary>
        public string SourceTableGroup { get; set; }

        public SourceTableGroup TableGroup {
            get {
                if (Enum.TryParse(SourceTableGroup, true, out SourceTableGroup group)) {
                    return group;
                }
                throw new Exception("Unknown source table group.");
            }
        }

        /// <summary>
        /// Specified whether the table definition is deprecated
        /// (i.e., only used for backwards compatibility)
        /// </summary>
        [XmlAttribute]
        public bool Deprecated { get; set; }

        /// <summary>
        /// Specified whether the table definition is experimental and should therefore
        /// not be visible by users of the system.
        /// </summary>
        [XmlAttribute]
        public bool IsExperimental { get; set; }

        public List<DataGroupTable> DataGroupTables { get; set; }

    }
}
