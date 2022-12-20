using MCRA.Utils.DataFileReading;
using MCRA.Data.Raw.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General.TableDefinitions {

    public partial class McraTableDefinitions {

        private static readonly object _instanceLocker = new object();
        private static McraTableDefinitions _instance;

        private static IDictionary<SourceTableGroup, DataGroupDefinition> _dataGroupDefinitions = null;
        private static IDictionary<RawDataSourceTableID,TableDefinition> _tableDefinitions = null;

        private static Dictionary<RawDataSourceTableID, TableFieldsMap> _tableLinksMap = null;

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraTableDefinitions Instance {
            get {
                if (_instance == null) {
                    lock (_instanceLocker) {
                        _instance = new McraTableDefinitions();
                    }
                }
                return _instance;
            }
        }

        private McraTableDefinitions() {
            _tableDefinitions = _loadTableDefinitions();
            _dataGroupDefinitions = _loadDataGroupDefinitions();
        }

        /// <summary>
        /// Dictionary of the table group definitions per source table group.
        /// </summary>
        public IDictionary<SourceTableGroup, DataGroupDefinition> DataGroupDefinitions {
            get {
                return _dataGroupDefinitions;
            }
        }

        /// <summary>
        /// Returns a flat list of all table definitions.
        /// </summary>
        public IDictionary<RawDataSourceTableID, TableDefinition> TableDefinitions {
            get {
                return _tableDefinitions;
            }
        }

        /// <summary>
        /// Get the table links map containing information on ordering of columns and references to other tables
        /// </summary>
        /// <param name="rawTableId"></param>
        /// <returns></returns>
        public IReadOnlyDictionary<RawDataSourceTableID, TableFieldsMap> TableFieldsMap {
            get {
                if (_tableLinksMap == null) {
                    _tableLinksMap = createTableLinksMap();
                }
                return _tableLinksMap;
            }
        }

        /// <summary>
        /// Returns the raw tables belonging to a table group.
        /// </summary>
        /// <param name="tableGroup"></param>
        /// <returns></returns>
        public HashSet<RawDataSourceTableID> GetTableGroupRawTables(SourceTableGroup tableGroup, string dataFormatId = null) {
            if (DataGroupDefinitions.TryGetValue(tableGroup, out var groupDefinition)) {
                var tableNames = string.IsNullOrEmpty(dataFormatId)
                    ? groupDefinition.DataGroupTables.Select(d => d.Id)
                    : groupDefinition.DataFormats.Where(f => f.Id == dataFormatId).SelectMany(f => f.TableIds);

                var tableIds = tableNames
                    .Select(n => {
                        if (Enum.TryParse(n, true, out RawDataSourceTableID rawId)) {
                            return rawId;
                        }
                        throw new Exception($"Table group {tableGroup} contains unknown data tables.");
                        //return RawDataSourceTableID.Unknown;
                    })
                    .Where(r => r != RawDataSourceTableID.Unknown);
                return new HashSet<RawDataSourceTableID>(tableIds);
            }
            return null;
        }

        /// <summary>
        /// Returns the table definition belonging to the table id.
        /// </summary>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public TableDefinition GetTableDefinition(RawDataSourceTableID tableId) {
            if (TableDefinitions.TryGetValue(tableId, out var result)) {
                return result;
            }
            return null;
        }

        private static IDictionary<SourceTableGroup, DataGroupDefinition> _loadDataGroupDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.TableDefinitions.DataGroupDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(DataGroupDefinitionCollection));
                var result = (DataGroupDefinitionCollection)xs.Deserialize(stream);
                return result.ToDictionary(r => r.TableGroup);
            }
        }

        private static IDictionary<RawDataSourceTableID, TableDefinition> _loadTableDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.TableDefinitions.TableDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(TableDefinitionCollection));
                var result = (TableDefinitionCollection)xs.Deserialize(stream);
                return result.ToDictionary(r => {
                    if (Enum.TryParse(r.Id, true, out RawDataSourceTableID rawId)) {
                        return rawId;
                    }
                    throw new Exception($"Failed to load table {r.Id}.");
                });
            }
        }

        private Dictionary<RawDataSourceTableID, TableFieldsMap> createTableLinksMap() {
            //fill the table links map from the table definitions
            var map = new Dictionary<RawDataSourceTableID, TableFieldsMap>();

            //reverse map dictionary
            var dict = RawTableIdToFieldEnums.IdToEnumMap;

            //iterate over table definitions
            foreach (var tdef in TableDefinitions.Values) {
                //get the table definition ID parsed as the RawDataSourceTableID enum value
                if (Enum.TryParse(tdef.Id, true, out RawDataSourceTableID rdsId)
                    //use this to get the corresponding RawTableFields enum type
                    && dict.TryGetValue(rdsId, out Type enumType)
                ) {
                    //get the desired field ordering based on the column definition's
                    //order ranks based on columns that have an OrderRank > 0
                    var orderFields = tdef.ColumnDefinitions
                        .Where(cd => cd.OrderRank > 0)
                        .OrderBy(cd => cd.OrderRank)
                        .Select(cd => (Enum)Enum.Parse(enumType, cd.Id, true))
                        .ToArray();

                    //get the referenced tables from the column definitions
                    //and store the column id's grouped by the referenced table id
                    var referenceFields = tdef.ColumnDefinitions
                        .Where(cd => cd.ForeignKeyTables != null)
                        .SelectMany(cd => cd.ForeignKeyTables, (cd, t) => {
                            var foreignTableId = (RawDataSourceTableID)Enum.Parse(typeof(RawDataSourceTableID), t, true);
                            //use field of current table: resolve to enum type of
                            //the current table's raw enum type
                            var field = (Enum)Enum.Parse(enumType, cd.Id, true);
                            return new { ForeignTableId = foreignTableId, Field = field };
                        })
                        .GroupBy(fc => fc.ForeignTableId)
                        .ToDictionary(g => g.Key, g => g.Select(s => s.Field).ToArray());

                    //Only for strong entities that are referred to: these entities
                    //require a single primary key column that uniquely identifies it
                    Enum primaryKeyFieldEnum = null;
                    Enum nameFieldEnum = null;
                    if (tdef.IsStrongEntity) {
                        //add own primary key to reference fields list for strong entities
                        //requires a single primary key column, so use First(...)
                        var keyField = tdef.ColumnDefinitions.First(cd => cd.IsPrimaryKey);
                        primaryKeyFieldEnum = (Enum)Enum.Parse(enumType, keyField.Id, true);

                        var nameField = tdef.ColumnDefinitions.FirstOrDefault(r => r.IsNameColumn);
                        if (nameField != null) {
                            nameFieldEnum = (Enum)Enum.Parse(enumType, nameField?.Id, true);
                        }
                    }

                    //add to the dictionary
                    map[rdsId] = new TableFieldsMap(enumType, referenceFields, orderFields) {
                        PrimaryKeyField = primaryKeyFieldEnum,
                        NameField = nameFieldEnum
                    };
                }
            }

            return map;
        }
    }
}
