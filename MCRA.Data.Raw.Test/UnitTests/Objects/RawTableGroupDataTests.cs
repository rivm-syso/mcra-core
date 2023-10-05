using MCRA.Data.Raw.Objects;
using MCRA.General;
using MCRA.General.TableDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace MCRA.Data.Raw.Test.UnitTests.Objects {

    [TestClass]
    public class RawTableGroupDataTests {

        private static HashSet<string> _tableRecordFieldTabuList = new() {
            "DoseResponseModels.DoseResponseModelType",
            "DoseResponseModels.ProastVersion",
            "DoseResponseModelBenchmarkDoses.Rpf",
            "DoseResponseModelBenchmarkDoses.RpfLower",
            "DoseResponseModelBenchmarkDoses.RpfUpper",
            "DoseResponseModelBenchmarkDosesUncertain.Rpf",
            "DoseResponseModelBenchmarkDosesUncertain.RpfLower",
            "DoseResponseModelBenchmarkDosesUncertain.RpfUpper",
        };

        [TestMethod]
        public void RawTableObjectsTest_TestIRawDataTableRecords() {
            var type = typeof(IRawDataTableRecord);
            var assembly = Assembly.GetAssembly(typeof(IRawDataTableRecord));
            var types = assembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type))
                .ToList();
            foreach (var recordType in types) {
                var tableId = RawDataSourceTableIDAttribute.GetRawDataSourceTableID(recordType);
                var tableDefinition = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                var typeProperties = recordType.GetProperties().ToDictionary(r => r.Name);

                // Check whether there is property for each column definition
                foreach (var field in tableDefinition.ColumnDefinitions) {
                    Assert.IsTrue(field.IsDynamic || field.Deprecated || typeProperties.ContainsKey(field.Id));
                }

                // Check whether there is property for each column definition
                var columnDefinitionLookup = tableDefinition.ColumnDefinitions.ToDictionary(r => r.Id);
                foreach (var field in typeProperties) {
                    var key = $"{tableDefinition.Id}.{field.Key}";
                    if (!_tableRecordFieldTabuList.Contains(key)) {
                        var columnDefinition = tableDefinition.FindColumnDefinitionByAlias(field.Key);
                        Assert.IsNotNull(columnDefinition);
                        if (columnDefinition.FieldType.Equals("AlphaNumeric", StringComparison.OrdinalIgnoreCase)) {
                            Assert.AreEqual(typeof(string), field.Value.PropertyType);
                        } else if (columnDefinition.FieldType.Equals("Numeric", StringComparison.OrdinalIgnoreCase)) {
                            if (!columnDefinition.Required) {
                                Assert.AreEqual(typeof(double?), field.Value.PropertyType);
                            } else {
                                Assert.AreEqual(typeof(double), field.Value.PropertyType);
                            }
                        } else if (columnDefinition.FieldType.Equals("Integer", StringComparison.OrdinalIgnoreCase)) {
                            if (!columnDefinition.Required) {
                                Assert.AreEqual(typeof(int?), field.Value.PropertyType);
                            } else {
                                Assert.AreEqual(typeof(int), field.Value.PropertyType);
                            }
                        } else if (columnDefinition.FieldType.Equals("DateTime", StringComparison.OrdinalIgnoreCase)) {
                            if (!columnDefinition.Required) {
                                Assert.AreEqual(typeof(DateTime?), field.Value.PropertyType);
                            } else {
                                Assert.AreEqual(typeof(DateTime), field.Value.PropertyType);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the factory recognizes all specified table groups.
        /// </summary>
        [TestMethod]
        public void RawTableGroupDataTests_TestRawDataTypes() {
            var tableGroups = Enum.GetValues(typeof(SourceTableGroup))
                .Cast<SourceTableGroup>()
                .Where(r => r != SourceTableGroup.Unknown);

            foreach (var tableGroup in tableGroups) {
                var rawTableGroupData = RawTableGroupDataFactory.Create(tableGroup);
                if (rawTableGroupData != null) {
                    foreach (var recordType in rawTableGroupData.RawTableRecordObjectTypes()) {
                        var tableDefinition = McraTableDefinitions.Instance.GetTableDefinition(recordType.Key);
                        var typeProperties = recordType.Value.GetProperties().ToDictionary(r => r.Name);

                        // Check whether there is property for each column definition
                        foreach (var field in tableDefinition.ColumnDefinitions) {
                            Assert.IsTrue(field.IsDynamic || typeProperties.ContainsKey(field.Id));
                        }

                        // Check whether there is property for each column definition
                        var columnDefinitionLookup = tableDefinition.ColumnDefinitions.ToDictionary(r => r.Id);
                        foreach (var field in typeProperties) {
                            var key = $"{tableDefinition.Id}.{field.Key}";
                            if (!_tableRecordFieldTabuList.Contains(key)) {
                                var columnDefinition = tableDefinition.FindColumnDefinitionByAlias(field.Key);
                                Assert.IsNotNull(columnDefinition);
                                if (columnDefinition.FieldType.Equals("AlphaNumeric", StringComparison.OrdinalIgnoreCase)) {
                                    Assert.AreEqual(typeof(string), field.Value.PropertyType);
                                } else if (columnDefinition.FieldType.Equals("Numeric", StringComparison.OrdinalIgnoreCase)) {
                                    if (!columnDefinition.Required) {
                                        Assert.AreEqual(typeof(double?), field.Value.PropertyType);
                                    } else {
                                        Assert.AreEqual(typeof(double), field.Value.PropertyType);
                                    }
                                } else if (columnDefinition.FieldType.Equals("Integer", StringComparison.OrdinalIgnoreCase)) {
                                    if (!columnDefinition.Required) {
                                        Assert.AreEqual(typeof(int?), field.Value.PropertyType);
                                    } else {
                                        Assert.AreEqual(typeof(int), field.Value.PropertyType);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test whether all classes that implement the IRawDataTableRecord interface
        /// (i.e., classes that should represent raw table records)
        /// </summary>
        [TestMethod]
        public void RawTableGroupDataTests_TestRawTableRecords() {
            var types = Assembly.GetAssembly(typeof(IRawDataTableRecord))
                .GetTypes()
                .Where(p => p != typeof(IRawDataTableRecord)
                    && typeof(IRawDataTableRecord).IsAssignableFrom(p))
                .ToList();

            foreach (var recordType in types) {
                var tableId = RawDataSourceTableIDAttribute.GetRawDataSourceTableID(recordType);
                var tableDefinition = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                var typeProperties = recordType.GetProperties().ToDictionary(r => r.Name);

                // Check whether there is property for each column definition
                foreach (var field in tableDefinition.ColumnDefinitions) {
                    Assert.IsTrue(field.Deprecated || field.IsDynamic || typeProperties.ContainsKey(field.Id), $"Missing field {field.Id} in raw object for table {tableId}.");
                }

                // Check whether there is property for each column definition
                var columnDefinitionLookup = tableDefinition.ColumnDefinitions.ToDictionary(r => r.Id);
                foreach (var field in typeProperties) {
                    var key = $"{tableDefinition.Id}.{field.Key}";
                    if (!field.Value.PropertyType.IsEnum && !_tableRecordFieldTabuList.Contains(key)) {
                        // Check whether the field matches any column id of the table definition
                        Assert.IsNotNull(tableDefinition.ColumnDefinitions.FirstOrDefault(r => r.Id == field.Key));
                    }
                }
            }
        }
    }
}
