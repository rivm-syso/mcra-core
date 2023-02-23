namespace MCRA.General.TableDefinitions {
    public class TableFieldsMap {

        /// <summary>
        /// Enumeration type of the table definition this instance is for
        /// For example typeof(RawTableFieldEnums.RawCompounds)
        /// </summary>
        public Type EnumType { get; }

        /// <summary>
        /// The enum value that represents the primary key for this table definition (if any)
        /// </summary>
        public Enum PrimaryKeyField { get; set; }

        /// <summary>
        /// The enum value that represents the primary key for this table definition (if any)
        /// </summary>
        public Enum NameField { get; set; }

        /// <summary>
        /// The enum values describing the field ordering of the returned table rows/entities
        /// These are individual Enum values of the EnumType
        /// </summary>
        public Enum[] OrderFields { get; }
        
        /// <summary>
        /// The foreign key references expressed as a dictionary of key-value pairs
        /// The key is the RawDataSourceTableID of the referenced table
        /// The Value is a list of Enum values with the field names of THIS tabledef, so from the
        /// EnumType of this instance, NOT the referred table
        /// The rule is that the referred table is always a strong entity having
        /// a SINGLE primary key which can be derived from the referred table id/definition itself
        /// </summary>
        public IReadOnlyDictionary<RawDataSourceTableID, Enum[]> ForeignKeyReferences { get; private set; } = new Dictionary<RawDataSourceTableID, Enum[]>();

        /// <summary>
        /// Constructor using no foreign keys
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="orderFields"></param>
        public TableFieldsMap(Type enumType, params Enum[] orderFields) {
            EnumType = enumType;
            OrderFields = orderFields;
        }

        /// <summary>
        /// constructor accepting dictionary of foreign key maps
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="references"></param>
        /// <param name="orderFields"></param>
        public TableFieldsMap(Type enumType, Dictionary<RawDataSourceTableID, Enum[]> references, params Enum[] orderFields) {
            ForeignKeyReferences = references;
            EnumType = enumType;
            OrderFields = orderFields;
        }

        /// <summary>
        /// Returns a string array of the reference field's names, derived from the Enum values of THIS instance
        /// and not the referred table's.
        /// </summary>
        /// <param name="referencedTable">The referenced table</param>
        /// <returns>String array of field names of THIS instance</returns>
        public string[] GetReferenceFieldNames(RawDataSourceTableID referencedTable) {
            if (ForeignKeyReferences != null && ForeignKeyReferences.TryGetValue(referencedTable, out Enum[] value)) {
                return value.Select(c => c.ToString()).ToArray();
            }
            return null;
        }
    }
}
