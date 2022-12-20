using System;
using System.Collections.Generic;
using System.Linq;
using MCRA.Utils.ExtensionMethods;
using MCRA.General.ModuleDefinitions;
using MCRA.General.SettingsDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.ModuleDefinitions {
    [TestClass]
    public class ModuleDefinitionTests {

        [TestMethod]
        public void ModuleDefinitions_TestCompletenessActionClasses() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var enumValues = Enum.GetValues(typeof(ActionClass))
                .Cast<ActionClass>();
            // Check whether there is a definition for each enum value.
            foreach (var value in enumValues) {
                var definition = definitionsInstance.ModuleGroupDefinitions.FirstOrDefault(r => r.ActionClass == value);
                Assert.IsNotNull(definition);
            }
        }

        [TestMethod]
        public void ModuleDefinitions_TestActionClassDisplayNames() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var enumValues = Enum.GetValues(typeof(ActionClass))
                .Cast<ActionClass>();
            // Check whether there is a definition for each enum value.
            foreach (var value in enumValues) {
                var definition = definitionsInstance.ModuleGroupDefinitions.FirstOrDefault(r => r.ActionClass == value);
                Assert.AreEqual(value.GetDisplayName(), definition.Name);
            }
        }

        [TestMethod]
        public void ModuleDefinitions_TestCompletenessActionTypes() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            var enumValues = Enum.GetValues(typeof(ActionType))
                .Cast<ActionType>()
                .Where(r => r != ActionType.Unknown);
            // Check whether there is a definition for each enum value.
            foreach (var value in enumValues) {
                var definition = definitionsInstance.ModuleDefinitions[value];
                Assert.IsNotNull(definition);
            }
        }

        [TestMethod]
        public void ModuleDefinitions_TestActionTypeDisplayNames() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            var enumValues = Enum.GetValues(typeof(ActionType))
                .Cast<ActionType>()
                .Where(r => r != ActionType.Unknown);
            // Check whether there is a definition for each enum value.
            foreach (var value in enumValues) {
                var definition = definitionsInstance.ModuleDefinitions[value];
                Assert.AreEqual(value.GetDisplayName(), definition.Name);
            }
        }

        [TestMethod]
        public void ModuleDefinitions_TestCompletenessTableGroups() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            foreach (var definition in definitions.Values) {
                if (!string.IsNullOrEmpty(definition.TableGroup)) {
                    SourceTableGroup value;
                    Assert.IsTrue(Enum.TryParse(definition.TableGroup, out value));
                }
            }
            var enumValues = Enum.GetValues(typeof(SourceTableGroup))
                .Cast<SourceTableGroup>()
                .Where(r => r != SourceTableGroup.Unknown)
                .Where(r => r != SourceTableGroup.DietaryExposures)
                .Where(r => r != SourceTableGroup.TargetExposures)
                .Where(r => r != SourceTableGroup.Risks)
                .ToList();
            foreach (var val in enumValues) {
                Assert.IsTrue(definitions.Any(r => val.ToString() == r.Value.TableGroup));
            }
        }

        [TestMethod]
        public void ModuleDefinitions_TestCompletenessActionClass() {
            var enumValues = Enum.GetValues(typeof(ActionType)).Cast<ActionType>().ToList();
            foreach (var value in enumValues) {
                if (value != ActionType.Unknown) {
                    var actionClass = McraModuleDefinitions.Instance.GetActionClass(value);
                }
            }
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ModuleDefinitions_TestTiers() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            foreach (var definition in definitions.Values) {
                var tiers = definition.Tiers;
                if (tiers?.Any() ?? false) {
                    foreach (var tier in tiers) {
                        foreach (var setting in tier.TierSettings) {
                            Assert.IsTrue(definition.AllModuleSettings.Contains(setting.IdSetting));
                        }
                        foreach (var inputTier in tier.InputTiers) {
                            Assert.IsTrue(definitionsInstance.ModuleDefinitionsById.TryGetValue(inputTier.Input, out var inputModule));
                            Assert.IsTrue(inputModule.Tiers.Any(r => r.Id == inputTier.Tier));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ModuleDefinitions_TestCanComputeCalculationModules() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions.Values
                .Where(r => r.ModuleType == ModuleType.CalculatorModule)
                .ToList();
            foreach (var definition in definitions) {
                Assert.IsTrue(definition.CanCompute);
            }
        }
    }
}
