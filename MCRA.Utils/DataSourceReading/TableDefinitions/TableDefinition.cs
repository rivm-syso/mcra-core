using System.Data;
using System.Xml.Serialization;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Holds a table definition that can be used to parse tables from a data source
    /// into a standard table entity.
    /// </summary>
    public class TableDefinition {

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
        /// Additional technical notes.
        /// </summary>
        [XmlArrayItem("TechnicalNote")]
        public List<string> TechnicalNotes { get; set; }

        /// <summary>
        /// Specifis whether the table definition is deprecated
        /// (i.e., only used for backwards compatibility).
        /// </summary>
        [XmlAttribute]
        public bool Deprecated { get; set; }

        /// <summary>
        /// Specifies whether the table definition is experimental and should therefore
        /// not be visible by users of the system.
        /// </summary>
        [XmlAttribute]
        public bool IsExperimental { get; set; }

        /// <summary>
        /// True if this table is a strong entity table, that is a table with a unique
        /// identifying primary key.
        /// </summary>
        [XmlAttribute]
        public bool IsStrongEntity { get; set; } = false;

        /// <summary>
        /// The aliases for this table definition.
        /// </summary>
        public HashSet<string> Aliases { get; set; } = new();

        /// <summary>
        /// The hidden aliases for this table definition.
        /// These are valid aliases, but they are deprecated and no longer visible
        /// in the documentation but kept for backwards compatibility.
        /// </summary>
        public HashSet<string> HiddenAliases { get; set; } = new();

        /// <summary>
        /// The name of the target table linked to this table definition.
        /// I.e., if this table definition is directly linked to an internal data table,
        /// then this is this is the name of that linked internal data table.
        /// </summary>
        public string TargetDataTable { get; set; }

        /// <summary>
        /// Specifies whether this table definition has a target table.
        /// </summary>
        public bool HasTargetDataTable {
            get {
                return !string.IsNullOrEmpty(TargetDataTable);
            }
        }

        /// <summary>
        /// The column definitions contained in this table definition.
        /// </summary>
        public List<ColumnDefinition> ColumnDefinitions { get; set; } = new();

        /// <summary>
        /// Searches for a column definition based on the provided search key.
        /// Note that this method ignores fallback aliases. It only returns the
        /// column definition for which this is a main column alias.
        /// </summary>
        /// <param name="columnAlias"></param>
        /// <returns></returns>
        public ColumnDefinition FindColumnDefinitionByAlias(string columnAlias) {
            var defs = ColumnDefinitions
                .Where(c => c.AcceptsHeader(columnAlias, false))
                .ToArray();

            if (defs.Length == 0) {
                return null;
            } else if (defs.Length == 1) {
                return defs[0];
            }

            // More than one found
            throw new DuplicateNameException($"Multiple column IDs are found for the alias '{columnAlias}'");
        }

        /// <summary>
        /// Returns the index of the column alias in the table definition's column collection.
        /// Note that this method ignores fallback aliases.It only returns the
        /// column definition for which this is a main column alias.
        /// </summary>
        /// <param name="columnAlias"></param>
        /// <returns>index of found column alias, or -1 if not found</returns>
        public int GetIndexOfColumnDefinitionByAlias(string columnAlias) {
            var index = 0;
            foreach (var colDef in ColumnDefinitions) {
                if (colDef.AcceptsHeader(columnAlias, false)) {
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Returns whether this table accepts the given name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool AcceptsName(string tableName) {
            return Id.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)
                || Aliases.Contains(tableName, StringComparer.InvariantCultureIgnoreCase)
                || HiddenAliases.Contains(tableName, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Returns the primary key column, or null if there is no primary key column.
        /// </summary>
        /// <returns></returns>
        public ColumnDefinition GetPrimaryKeyColumn() {
            return ColumnDefinitions.SingleOrDefault(c => c.IsPrimaryKey);
        }

        /// <summary>
        /// Readable string format.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Tabledef {Id} - {Name}";
        }
    }
}
