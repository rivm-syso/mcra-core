using System.Collections.ObjectModel;
using System.Data;
using System.Xml.Serialization;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// A collection of TableDefinition objects. Provides several methods to search through its items in a convenient way.
    /// Can be (de)serialized from and to xml.
    /// </summary>
    [Serializable()]
    [XmlRoot("TableDefinitions")]
    public sealed class TableDefinitionCollection : Collection<TableDefinition> {

        /// <summary>
        /// Searches through the collection and returns the TableDefinition that
        /// has the target Alias-name in it's Aliases collection property.
        /// </summary>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        public TableDefinition GetDefinitionByAlias(string tableAlias) {
            var defs = this.Where(t => t.AcceptsName(tableAlias)).ToArray();
            if (defs.Length == 0) {
                return null;
            } else if (defs.Length == 1) {
                return defs[0];
            } else {
                // More than one found
                throw new DuplicateNameException($"Multiple table IDs are found for the alias '{tableAlias}'");
            }
        }

        /// <summary>
        /// Searches the collection for a TableDefinition with the target id.
        /// </summary>
        /// <param name="tableID">The TableID to seach for.</param>
        /// <returns>The TableDefinition with the target TableID, or null in case
        /// no such TableDefinition is found.</returns>
        public TableDefinition GetDefinitionByTableID(string tableID) {
            return this.FirstOrDefault(td => td.Id.Equals(tableID, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Searches the collection for a TableDefinition with the target TableID
        /// (based on a ToString conversion of the enum value).
        /// </summary>
        /// <param name="value">The TableID to seach for.</param>
        /// <returns>The TableDefinition with the target TableID, or null in case
        /// no such TableDefinition is found.</returns>
        public TableDefinition GetDefinitionByTableID(Enum value) {
            return this.FirstOrDefault(td => td is TableDefinition && td.Id.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
