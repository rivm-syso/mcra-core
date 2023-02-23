using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace MCRA.General.Test.UnitTests.ModuleDefinitions {
    [TestClass]
    public class KineticModelDefinitionTests {

        /// <summary>
        /// Test uniqueness of parameter definitions.
        /// </summary>
        [TestMethod]
        public void KineticModelDefinition_TestUniquenessParameterDefinitions() {
            var kineticModels = MCRAKineticModelDefinitions.Definitions;
            foreach (var kineticModel in kineticModels.Values) {
                var parameterDefinitions = kineticModel.Parameters
                    .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
                Assert.IsNotNull(parameterDefinitions);
            }
        }

        /// <summary>
        /// Check whether there is a unit definition for each enum value and
        /// check whether each unit specified in the definitions matches an enum value.
        /// </summary>
        [TestMethod]
        public void KineticModelDefinition_TestCompleteness() {
            var definition = MCRAKineticModelDefinitions.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(KineticModelType))
                .Cast<KineticModelType>()
                .Where(r => r != KineticModelType.Undefined);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<KineticModelType>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(KineticModelType), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void KineticModelDefinition_TestAliases() {
            var definition = MCRAKineticModelDefinitions.UnitDefinition;
            var aliases = definition.Units
                .SelectMany(r => r.Aliases, (r, a) => new {
                    Unit = Enum.Parse(typeof(KineticModelType), r.Id),
                    Alias = a
                })
                .ToList();
            // Check whether the parsed unit for each alias matches with the amount
            // unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = MCRAKineticModelDefinitions.FromString(alias.Alias);
                Assert.AreEqual(alias.Unit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void KineticModelDefinition_TestGetByAlias() {
            var definition = MCRAKineticModelDefinitions.UnitDefinition;
            var aliases = definition.Units
                .SelectMany(r => r.Aliases)
                .ToList();

            // Check whether the parsed unit for each alias matches with the amount
            // unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                Assert.IsTrue(MCRAKineticModelDefinitions.TryGetDefinitionByAlias(alias.ToLower(), out var modelDefinition));
                Assert.IsNotNull(modelDefinition);
            }

            // Check whether we can also get the model definition with the IDs.
            var ids = definition.Units.Select(r => r.Id);
            foreach (var id in ids) {
                Assert.IsTrue(MCRAKineticModelDefinitions.TryGetDefinitionByAlias(id.ToLower(), out var modelDefinition));
                Assert.IsNotNull(modelDefinition);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void KineticModelDefinition_TestDisplayNames() {
            var definition = MCRAKineticModelDefinitions.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (KineticModelType)Enum.Parse(typeof(KineticModelType), units.Id);
                Assert.AreEqual(units.Name, value.GetDisplayName(), true, CultureInfo.InvariantCulture);
                Assert.AreEqual(units.ShortName, value.GetShortDisplayName(), true, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Check whether the ID of a model definition has the form MODELID_VERSION.
        /// </summary>
        [TestMethod]
        public void KineticModelDefinition_TestIdentifier() {
            var definition = MCRAKineticModelDefinitions.Definitions;
            foreach (var units in definition.Values) {
                Assert.AreEqual(units.Id, $"{units.IdModel}_{units.Version}");
            }
        }
    }
}
