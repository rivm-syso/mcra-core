using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace MCRA.Utils.DataFileReading {

    /// <summary>
    /// Definitions of the SourceDataTable Groups that exist
    /// </summary>
    public enum FieldType {
        [Display(Name = "Undefined")]
        Undefined,
        [Display(Name = "AlphaNumeric")]
        AlphaNumeric,
        [Display(Name = "Numeric")]
        Numeric,
        [Display(Name = "Boolean")]
        Boolean,
        [Display(Name = "Integer")]
        Integer,
        [Display(Name = "DateTime")]
        DateTime
    }

    /// <summary>
    /// Holds the definition of a column that can be used to parse columns of a
    /// data source table into an entity table.
    /// </summary>
    public sealed class ColumnDefinition {

        /// <summary>
        /// The ID of the column.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the column.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the column.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Additional technical notes.
        /// </summary>
        [XmlArrayItem("TechnicalNote")]
        public List<string> TechnicalNotes { get; set; }

        /// <summary>
        /// Bool that specifies whether the column is a primary key column.
        /// </summary>
        [XmlAttribute]
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Bool that specifies whether the values of this column should be unique.
        /// </summary>
        [XmlAttribute]
        public bool IsUnique { get; set; }

        /// <summary>
        /// Specifies whether this field holds a name.
        /// </summary>
        [XmlAttribute]
        public bool IsNameColumn { get; set; }

        /// <summary>
        /// Bool that specified whether this column is required.
        /// </summary>
        [XmlAttribute]
        public bool Required { get; set; }

        /// <summary>
        /// The default value of this column if it is not specified, returns the
        /// raw string value of the XML attribute.
        /// Only useful when Required == false.
        /// </summary>
        [XmlAttribute]
        public string DefaultValue { get; set; }

        /// <summary>
        /// The rank of the column in the ordering of the table definition
        /// starting with 1, default is 0, this means it's not ordered on this column
        /// </summary>
        [XmlAttribute]
        public int OrderRank { get; set; } = 0;

        /// <summary>
        /// The field size of the values of this column.
        /// </summary>
        [XmlAttribute]
        public int FieldSize { get; set; } = -1;

        /// <summary>
        /// The field type of this column.
        /// </summary>
        [XmlAttribute]
        public string FieldType { get; set; }

        /// <summary>
        /// A bool that specifies whether this column is a dynamic column.
        /// </summary>
        [XmlAttribute]
        public bool IsDynamic { get; set; }

        /// <summary>
        /// A bool that specifies whether this column is deprecated.
        /// </summary>
        [XmlAttribute]
        public bool Deprecated { get; set; }

        /// <summary>
        /// The alias identifiers of this column.
        /// </summary>
        public HashSet<string> Aliases { get; set; } = new HashSet<string>();

        /// <summary>
        /// The hidden aliases for this column definition.
        /// These are valid aliases, but they are deprecated and no longer visible
        /// in the documentation but kept for backwards compatibility.
        /// </summary>
        public HashSet<string> HiddenAliases { get; set; } = new HashSet<string>();

        /// <summary>
        /// Fallback aliases are the field names that are also accepted when
        /// no suitable alias has been found.
        /// </summary>
        public HashSet<string> FallbackAliases { get; set; } = new HashSet<string>();

        /// <summary>
        /// List of foreign key references to tables
        /// The tables mentioned should be strong entities, normally only one
        /// table is referred, however it's possible to list multiple tables
        /// </summary>
        public List<string> ForeignKeyTables { get; set; }

        /// <summary>
        /// Returns whether this column accepts the given header name.
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="includeFallbackAliases">Specifies whether to also check the fallback aliases.</param>
        /// <returns></returns>
        public bool AcceptsHeader(string headerName, bool includeFallbackAliases) {
            return Id.Equals(headerName, StringComparison.InvariantCultureIgnoreCase)
                || Aliases.Contains(headerName, StringComparer.InvariantCultureIgnoreCase)
                || HiddenAliases.Contains(headerName, StringComparer.InvariantCultureIgnoreCase)
                || (includeFallbackAliases && FallbackAliases.Contains(headerName, StringComparer.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Finds the matching header index for the column definition.
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="checkRequired">When no match is found, check whether this field is required.</param>
        /// <returns></returns>
        public int GetMatchingHeaderIndex(List<string> headers, bool checkRequired = true) {
            if (IsDynamic) {
                // Dynamic properties don't match
                return -1;
            }
            var matches = headers
                .Select((h, ix) => (header: h, index: ix))
                .Where(r => AcceptsHeader(r.header, false))
                .ToList();
            if (!matches.Any()) {
                // Include fallback aliases if no match found
                matches = headers
                    .Select((h, ix) => (header: h, index: ix))
                    .Where(r => AcceptsHeader(r.header, true))
                    .ToList();
            }
            if (!matches.Any()) {
                if (checkRequired && Required) {
                    throw new Exception($"No matching table field found for column '{Id}' (required).");
                }
            } else if (matches.Count() > 1) {
                throw new Exception($"Found multiple matching table fields for column '{Id}'.");
            } else if (matches.Count == 1) {
                return matches.First().index;
            }
            return -1;
        }

        /// <summary>
        /// Returns the field type of the column.
        /// </summary>
        /// <returns></returns>
        public FieldType GetFieldType() {
            if (Enum.TryParse(FieldType, out FieldType fieldType)) {
                return fieldType;
            } else {
                return DataFileReading.FieldType.Undefined;
            }
        }
    }
}
