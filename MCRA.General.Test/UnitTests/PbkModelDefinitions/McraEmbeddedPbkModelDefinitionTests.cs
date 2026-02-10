namespace MCRA.General.Test.UnitTests.ModuleDefinitions {
    [TestClass]
    public class McraEmbeddedPbkModelDefinitionTests {

        /// <summary>
        /// Test uniqueness of parameter definitions.
        /// </summary>
        [TestMethod]
        public void McraEmbeddedPbkModelDefinitions_TestUniquenessParameterDefinitions() {
            var kineticModels = McraEmbeddedPbkModelDefinitions.Definitions;
            foreach (var kineticModel in kineticModels.Values) {
                var parameterDefinitions = kineticModel.Parameters
                    .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
                Assert.IsNotNull(parameterDefinitions);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void McraEmbeddedPbkModelDefinitions_TestGetByAlias() {
            var definition = McraEmbeddedPbkModelDefinitions.Definitions;
            var aliases = definition.Values
                .SelectMany(r => r.Aliases)
                .ToList();

            // Check whether the parsed unit for each alias matches with the amount
            // unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                Assert.IsTrue(McraEmbeddedPbkModelDefinitions.TryGetDefinitionByAlias(alias.ToLower(), out var modelDefinition));
                Assert.IsNotNull(modelDefinition);
            }

            // Check whether we can also get the model definition with the IDs.
            var ids = definition.Values.Select(r => r.Id);
            foreach (var id in ids) {
                Assert.IsTrue(McraEmbeddedPbkModelDefinitions.TryGetDefinitionByAlias(id.ToLower(), out var modelDefinition));
                Assert.IsNotNull(modelDefinition);
            }
        }

        /// <summary>
        /// Check whether the ID of a model definition has the form MODELID_VERSION.
        /// </summary>
        [TestMethod]
        public void McraEmbeddedPbkModelDefinitions_TestIdentifier() {
            var definition = McraEmbeddedPbkModelDefinitions.Definitions;
            foreach (var units in definition.Values) {
                Assert.AreEqual(units.Id, $"{units.IdModel}_{units.Version}");
            }
        }
    }
}
