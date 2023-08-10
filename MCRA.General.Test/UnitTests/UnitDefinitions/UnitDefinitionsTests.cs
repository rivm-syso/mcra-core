using MCRA.General.SettingsDefinitions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                "GenderType",
                "MonthType",
                "IscedType"
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
                "KineticModelType"
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
    }
}
