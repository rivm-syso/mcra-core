using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MCRA.General.SettingsDefinitions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
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
                "MonthType"
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

        /// <summary>
        /// Check whether enums in table definitions have a unit definition
        /// (and are therefore printed in the documentation).
        /// </summary>
        [TestMethod]
        public void UnitDefinitions_TestCompletenessUnitDefinitionsWithT4Template() {
            var asm = typeof(McraUnitDefinitions).Assembly;

            var enumTypes = asm.GetTypes()
                .Where(t => t.IsEnum)
                .Where(t => t.Namespace.Equals("MCRA.General.UnitDefinitions.UnitTypeEnums"));

            foreach (var newEnumType in enumTypes) {
                var oldEnumType = asm.GetType($"MCRA.General.{newEnumType.Name}");
                if(oldEnumType == null) {
                    Debug.WriteLine($"{newEnumType.Name} does not exist!");
                    continue;
                }
                var oldEnumValues = Enum.GetNames(oldEnumType)
                    .ToDictionary(k => k, k => oldEnumType.GetMember(k).First());

                var newEnumValues = Enum.GetNames(newEnumType)
                    .ToDictionary(k => k, k => newEnumType.GetMember(k).First());

                foreach (var name in oldEnumValues.Keys) {
                    var oldMember = oldEnumValues[name];

                    if(!newEnumValues.TryGetValue(name, out var newMember)) {
                        Debug.WriteLine($"{newEnumType.Name}.{name} does not exist in generated enum");
                        continue;
                    }

                    var oldValue = (int)Enum.Parse(oldEnumType, name, true);
                    var newValue = (int)Enum.Parse(newEnumType, name, true);

                    if(oldValue != newValue) {
                        Debug.WriteLine($"{oldEnumType.Name}.{name}={oldValue} does not match {newValue}");
                    }
                    if (oldMember.GetDisplayName() != newMember.GetDisplayName()) {
                        Debug.WriteLine($"{oldEnumType.Name}.{name} 'Name' should be {oldMember.GetDisplayName()}");
                    }
                    if (oldMember.GetShortName() != newMember.GetShortName()) {
                        Debug.WriteLine($"{oldEnumType.Name}.{name} 'ShortName' should be {oldMember.GetShortName()}");
                    }
                    var oldDesc = Regex.Replace(oldMember.GetDescription() ?? "", @"\s+", " ").Trim();
                    var newDesc = Regex.Replace(newMember.GetDescription() ?? "", @"\s+", " ").Trim();
                    if (oldDesc != newDesc && !string.IsNullOrWhiteSpace(oldDesc)) {
                        Debug.WriteLine($"{oldEnumType.Name}.{name} Description '{newDesc}' should be '{oldDesc}'");
                    }
                }
            }

            var convTypes = asm.GetTypes()
                .Where(t => t.IsClass)
                .Where(t => !string.IsNullOrEmpty(t.Namespace))
                .Where(t => t.Namespace.Equals("MCRA.General.UnitDefinitions.UnitTypeConverters"));

            foreach (var convType in convTypes) {
                var oldType = asm.GetType($"MCRA.General.{convType.Name}");
                Assert.IsNotNull(oldType);
            }
        }
    }
}
