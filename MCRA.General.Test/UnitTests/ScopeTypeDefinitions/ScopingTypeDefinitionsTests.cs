using MCRA.General.ScopingTypeDefinitions;
using MCRA.General.TableDefinitions;

namespace MCRA.General.Test.UnitTests.ScopeTypeDefinitions {

    [TestClass]
    public class ScopingTypeDefinitionsTests {

        [TestMethod]
        public void ScopingTypeDefinitions_TestCompleteness() {
            var definitionsInstance = McraScopingTypeDefinitions.Instance;
            var definitions = definitionsInstance.ScopingDefinitions;
            var enumValues = Enum.GetValues(typeof(ScopingType))
                .Cast<ScopingType>()
                .Where(r => r != ScopingType.Unknown);
            // Check whether there is a definition for each enum value.
            foreach (var value in enumValues) {
                var definition = definitionsInstance.ScopingDefinitions[value];
                Assert.IsNotNull(definition);

                // Although it is nullable, we always expect a value.
                Assert.IsNotNull(definition.RawTableId);
            }
        }

        [TestMethod]
        public void ScopingTypeDefinitions_TestParentScopes() {
            var definitions = McraScopingTypeDefinitions.Instance.ScopingDefinitions;
            foreach (var definition in definitions.Values) {
                var tableDefinition = McraTableDefinitions.Instance.GetTableDefinition(definition.RawTableId ?? RawDataSourceTableID.Unknown);
                foreach (var reference in definition.ParentScopeReferences) {
                    var field = tableDefinition.ColumnDefinitions.FirstOrDefault(r => r.Id == reference.IdField);
                    Assert.IsNotNull(field);
                }
                if (tableDefinition != null) {
                    foreach (var parentScopingReference in definition.ParentScopeReferences) {
                        var linkedScope = McraScopingTypeDefinitions.Instance.ScopingDefinitions[parentScopingReference.ReferencedScope];
                        var linkedTableId = linkedScope.RawTableId;

                        // Assert that the linked scope is linked to a table definition
                        Assert.IsNotNull(linkedTableId);

                        // Assert that the linking column exists
                        var linkingColumn = tableDefinition.ColumnDefinitions.FirstOrDefault(r => r.Id == parentScopingReference.IdField);
                        Assert.IsNotNull(linkingColumn);

                        // Assert that the foreign key reference is also defined in the table/column definition
                        Assert.Contains(linkedTableId.ToString(), linkingColumn.ForeignKeyTables);
                    }
                }
            }
        }

        [TestMethod]
        public void ScopingTypeDefinitions_TestConsistencyWithTableDefinitions() {
            var definitions = McraScopingTypeDefinitions.Instance.ScopingDefinitions.Values
                .GroupBy(r => r.TableGroup);
            foreach (var group in definitions) {
                var scopingTypeDefinitions = group.ToList();
                var tableGroupDefinition = McraTableDefinitions.Instance.DataGroupDefinitions[group.Key];

                // The target tables according to the data group definition
                var tableGroupTables = McraTableDefinitions.Instance
                    .GetTableGroupRawTables(group.Key)
                    .ToDictionary(r => r, r => McraTableDefinitions.Instance.TableDefinitions[r]);

                // Check if all table-references in the scoping definitions align with the table definitions
                foreach (var definition in scopingTypeDefinitions) {
                    if (definition.RawTableId != null) {
                        var tableId = (RawDataSourceTableID)definition.RawTableId;
                        if (McraTableDefinitions.Instance.TableDefinitions.TryGetValue((RawDataSourceTableID)definition.RawTableId, out var tableDefinition)) {
                            Assert.AreEqual(definition.IsStrongEntity, tableDefinition.IsStrongEntity);
                        }

                        // Check if the table is included in the data group definition
                        // TODO: find a more generic way to omit KineticModelDefinitions
                        if (tableId != RawDataSourceTableID.KineticModelDefinitions) {
                            Assert.IsTrue(tableGroupTables.ContainsKey(tableId));
                        }
                    }
                }

                // Get data group tables and check whether all tables with a target table have a scoping type
                foreach (var table in tableGroupTables) {
                    if (table.Value.HasTargetDataTable) {
                        Assert.IsTrue(scopingTypeDefinitions.Any(r => (RawDataSourceTableID)r.RawTableId == table.Key),$"Scoping type \"{table.Key}\" is missing.");
                    }
                }
            }
        }
    }
}
