using MCRA.Utils.DataFileReading;
using MCRA.General.TableDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace MCRA.General.Test.UnitTests.TableDefinitions {
    [TestClass]
    public class McraTableDefinitionsTests {

        /// <summary>
        /// Check whether there is a table definition for each table enum value.
        /// </summary>
        [TestMethod]
        public void McraTableDefinitions_TestCompletenessTableDefinitions() {
            var definitionsInstance = McraTableDefinitions.Instance;
            var definitions = definitionsInstance.TableDefinitions;
            var enumValues = Enum.GetValues(typeof(RawDataSourceTableID))
                .Cast<RawDataSourceTableID>()
                .Where(r => r != RawDataSourceTableID.Unknown && r != RawDataSourceTableID.KineticModelDefinitions);

            foreach (var value in enumValues) {
                var definition = definitionsInstance.GetTableDefinition(value);
                Assert.IsNotNull(definition);
            }
        }

        /// <summary>
        /// Check whether all strong entity tables have a primary key, and the others
        /// don't.
        /// </summary>
        [TestMethod]
        public void McraTableDefinitions_TestPrimaryKeysOfStrongEntities() {
            var definitionsInstance = McraTableDefinitions.Instance;
            var definitions = definitionsInstance.TableDefinitions;
            foreach (var definition in definitions) {
                if (definition.Value.IsStrongEntity) {
                    Assert.IsTrue(definition.Value.ColumnDefinitions.Any(r => r.IsPrimaryKey));
                } else {
                    Assert.IsTrue(!definition.Value.ColumnDefinitions.Any(r => r.IsPrimaryKey));
                }
            }
        }

        /// <summary>
        /// Check whether there is a table group definition for each table group enum value.
        /// </summary>
        [TestMethod]
        public void McraTableDefinitions_TestCompletenetssDataGroupDefinitions() {
            var definitionsInstance = McraTableDefinitions.Instance;
            var definitions = definitionsInstance.DataGroupDefinitions;
            Assert.IsNotNull(definitions);

            var enumValues = Enum.GetValues(typeof(SourceTableGroup))
                .Cast<SourceTableGroup>()
                .Where(r => r != SourceTableGroup.Unknown);

            // Check whether each table group enum has a data group definition.
            foreach (var tableGroup in enumValues) {
                Assert.IsTrue(definitions.ContainsKey(tableGroup));
            }
        }

        /// <summary>
        /// Check the field types of all non-dynamic column definitions of
        /// all table definitions. Check whether the field type of the column 
        /// definition maps  a field type enum value, or a unit or type definition.
        /// </summary>
        [TestMethod]
        public void McraTableDefinitions_TestColumnFieldTypes() {
            var instances = McraTableDefinitions.Instance;
            var assembly = Assembly.Load("MCRA.General");

            // TODO: this tabu list should not exist. These are all inconsistencies
            // that should be resolved. Each field type should match either one of the
            // field-type enum values, or map to a unit or type definition.
            var tabuList = new HashSet<string>() {
                "ValueQualifier",
                "AgeUnit"
            };
            foreach (var tableDefinition in instances.TableDefinitions.Values) {
                foreach (var columnDefinition in tableDefinition.ColumnDefinitions) {
                    if (!columnDefinition.IsDynamic) {
                        if (!Enum.TryParse(columnDefinition.FieldType, out FieldType fieldType)) {
                            if (!tabuList.Contains(columnDefinition.FieldType)) {
                                var classType = assembly.GetType($"MCRA.General.{columnDefinition.FieldType}");
                                Assert.IsTrue(classType.IsEnum);
                            }
                            Assert.AreEqual(-1, columnDefinition.FieldSize, $"Error in field {columnDefinition.Id} of table {tableDefinition.Id}: field type should not be defined for units and controlled terminology types.");
                        } else {
                            Assert.IsTrue(fieldType != FieldType.Undefined);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void McraTableDefinitions_TestForeignKeyReferences() {
            var instances = McraTableDefinitions.Instance;
            foreach (var tableDefinition in instances.TableDefinitions.Values) {
                foreach (var columnDefinition in tableDefinition.ColumnDefinitions) {
                    foreach (var foreignKey in columnDefinition.ForeignKeyTables) {
                        var isValid = Enum.TryParse(foreignKey, out RawDataSourceTableID _);
                        Assert.IsTrue(isValid);
                    }
                }
            }
        }

        /// <summary>
        /// Tests for table definitions that have the target data table field specified, whether
        /// this target table is also included as a hidden alias. This is required.
        /// </summary>
        [TestMethod]
        public void McraTableDefinitions_TestTargetDataTablesAreInHiddenAliases() {
            var instances = McraTableDefinitions.Instance;
            foreach (var tableDefinition in instances.TableDefinitions.Values) {
                if (!string.IsNullOrEmpty(tableDefinition.TargetDataTable)) {
                    Assert.IsTrue(
                        tableDefinition.HiddenAliases.Contains(tableDefinition.TargetDataTable),
                        $"Target table id should be included as an alias in table definition {tableDefinition.Id}"
                    );
                }
            }
        }

        /// <summary>
        /// Make column name and description optional for strong entities
        /// </summary>
        [TestMethod]
        public void McraTableDefinitions_TestEntitiesForNameAndDescriptionOptional() {
            var definitionsInstance = McraTableDefinitions.Instance;
            var definitions = definitionsInstance.TableDefinitions;
            foreach (var definition in definitions) {
                if (definition.Value.IsStrongEntity) {
                    if (definition.Value.ColumnDefinitions.Any(r => r.IsPrimaryKey)){
                        foreach (var columnDef in definition.Value.ColumnDefinitions) {
                            if (columnDef.Id == "Name" && !columnDef.IsPrimaryKey) {
                                Assert.IsFalse(columnDef.Required, $"Name in table {definition.Value} has attribute Required = true");
                                Assert.IsTrue(columnDef.IsNameColumn, $"Name in table {definition.Value} has missing attribute IsNameColumn = true");
                            }
                            if (columnDef.Id == "Description") {
                                Assert.IsFalse(columnDef.Required, $"Description in table {definition.Value} has attribute Required = true");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if column name and description are present for strong entities
        /// </summary>
        [TestMethod]
        public void McraTableDefinitions_TestEntitiesForNameAndDescription() {
            var definitionsInstance = McraTableDefinitions.Instance;
            var definitions = definitionsInstance.TableDefinitions;
            var nameList = new List<string>();
            var descriptionList = new List<string>();
            foreach (var definition in definitions) {
                if (definition.Value.IsStrongEntity) {
                    if (definition.Value.ColumnDefinitions.Any(r => r.IsPrimaryKey)) {
                        if (!definition.Value.ColumnDefinitions.Select(c => c.Id).Contains("Name")) {
                            nameList.Add(definition.Value.ToString());
                        }
                        if (!definition.Value.ColumnDefinitions.Select(c => c.Id).Contains("Description")) {
                            descriptionList.Add(definition.Value.ToString());
                        }
                    }
                }
            }
            Assert.AreEqual(0, nameList.Count);
            Assert.AreEqual(0, descriptionList.Count);
        }


        [TestMethod]
        public void McraTableDefinitions_TestDataFormats() {
            var groupDefinitions = McraTableDefinitions.Instance.DataGroupDefinitions;
            foreach (var groupDefinition in groupDefinitions.Values) {
                if (groupDefinition.DataFormats?.Any() ?? false) {
                    var formats = groupDefinition.DataFormats.ToList();
                    // Check unique ids
                    var formatIds = formats.Select(r => r.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    Assert.AreEqual(formats.Count(), formatIds.Count());
                    // Check referenced tables
                    foreach (var format in formats) {
                        foreach (var tableId in format.TableIds) {
                            Assert.IsTrue(Enum.TryParse(tableId, out RawDataSourceTableID table));
                        }
                    }
                }
            }
        }
    }
}
