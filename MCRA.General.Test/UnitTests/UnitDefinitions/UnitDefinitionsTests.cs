using MCRA.General.SettingsDefinitions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

namespace MCRA.General.Test.UnitTests.UnitDefinitions {
    [TestClass]
    public class UnitDefinitionsTests {

        /// <summary>
        /// Check whether there is a unit definition that is not used in table definitions.
        /// </summary>
        [TestMethod]
        public void UnitDefinitions_TestUnUsedUnitDefinitions() {
            var definitionsInstance = McraUnitDefinitions.Instance;
            var definitions = definitionsInstance.UnitDefinitions;

            var tableDefs = McraTableDefinitions.Instance;
            var tableFieldTypes = tableDefs.TableDefinitions.Values
                .SelectMany(c => c.ColumnDefinitions
                    .Where(d => !d.IsDynamic)
                    .Select(d => d.FieldType))
                .ToHashSet();

            var settingsDefs = McraSettingsDefinitions.Instance;
            var settingsFieldTypes = settingsDefs.SettingsDefinitions
                .Select(d => d.Value.ValueType)
                .ToHashSet();

            // TODO: this tabu list should not exist.
            // These are all inconsistencies that should be resolved.
            var tabuList = new HashSet<string>() {
                "BooleanType",
                "ConcentrationValueType",
                "DoseResponseModelType",
                "PbkModelCompartmentType",
                "PbkModelParameterType",
                "PbkModelSpeciesType",
                "GenderType",
                "MonthType",
                "JobTaskType",
                "IscedType",
                "ExpressionType"
            };
            foreach (var definition in definitions) {
                if (!tabuList.Contains(definition.Key) &&
                    !tableFieldTypes.Contains(definition.Key) &&
                    !settingsFieldTypes.Contains(definition.Key)
                ) {
                    Assert.Fail($"Enum {definition.Key} is not used in any of the table or settings definitions XML.");
                }
            }
        }

        /// <summary>
        /// Check whether enums in table definitions have a unit definition
        /// (and are therefore printed in the documentation).
        /// </summary>
        [TestMethod]
        public void UnitDefinitions_TestCompletenessUnitDefinitions() {
            var definitionsInstance = McraUnitDefinitions.Instance;
            var definitions = definitionsInstance.UnitDefinitions;
            var instances = McraTableDefinitions.Instance;

            // TODO: this tabu list should not exist.
            // These are all inconsistencies that should be resolved.
            var tabuList = new HashSet<string>() {
                "PbkModelType"
            };

            foreach (var tableDefinition in instances.TableDefinitions.Values) {
                foreach (var columnDefinition in tableDefinition.ColumnDefinitions) {
                    if (!columnDefinition.IsDynamic) {
                        if (!Enum.TryParse(columnDefinition.FieldType, out FieldType _)) {
                            var typeString = columnDefinition.FieldType;
                            if (!tabuList.Contains(columnDefinition.FieldType)) {
                                if (!definitions.ContainsKey(typeString)) {
                                    Assert.Fail($"Fieldtype {columnDefinition.FieldType} in {tableDefinition.Id} is not defined in unit-definitions.");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check whether Id is also in the list of aliases. This is a consistency check to ensure that the Id is always 
        /// present in the list of aliases.
        /// </summary>
        [TestMethod]
        public void UnitDefinitions_TestAliases() {
            var definitionsInstance = McraUnitDefinitions.Instance;
            var unitDefinitions = definitionsInstance.UnitDefinitions
                .Where(c => !c.Value.IsInternalEnum)
                .ToDictionary(c => c.Key, c => c.Value);
            var definitionKeys = new List<string>();
            foreach (var definition in unitDefinitions) {
                var units = definition.Value.Units;
                foreach (var unit in units) {
                    if (!unit.Aliases.Contains(unit.Id)) {
                        definitionKeys.Add(definition.Key);
                    }
                }
            }
            var definitions = definitionKeys.ToHashSet();
            if (definitions.Count > 0) {
                Assert.Fail($"Ids are not in the list of aliases.");
            }
        }

        /// <summary>
        /// Check whether aliases are unique for each unit.
        /// </summary>
        [TestMethod]
        public void UnitDefinitions_TestDoubleAliases() {
            var definitionsInstance = McraUnitDefinitions.Instance;
            var unitDefinitions = definitionsInstance.UnitDefinitions
                .Where(c => !c.Value.IsInternalEnum)
                .ToDictionary(c => c.Key, c => c.Value);
            foreach (var definition in unitDefinitions) {
                var units = definition.Value.Units;
                foreach (var unit in units) {
                    var aliasCount = unit.Aliases.Count();
                    var uniqueAliasCount = unit.Aliases.ToHashSet().Count();
                    Assert.AreEqual(aliasCount, uniqueAliasCount);
                }
            }
        }

        /// <summary>
        /// Check whether the first alias is equal to the Id of the unit.
        /// This is a consistency check to ensure that the first alias is always equal to the Id of the unit.
        /// </summary>
        [TestMethod]
        public void UnitDefinitions_TestFirstAliasId() {
            var definitionsInstance = McraUnitDefinitions.Instance;
            var unitDefinitions = definitionsInstance.UnitDefinitions
                .Where(c => !c.Value.IsInternalEnum)
                .ToDictionary(c => c.Key, c => c.Value);
            var results = new List<string>();
            foreach (var definition in unitDefinitions) {
                var units = definition.Value.Units;
                foreach (var unit in units) {
                    var firstAlias = unit.Aliases.First();
                    var id = unit.Id;
                    if (firstAlias != id) {
                        results.Add(firstAlias);
                    }
                }
            }
            if (results.Count > 0) {
                Assert.Fail($"Id is not the first alias.");
            }
        }
    }
}
