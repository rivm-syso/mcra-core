using MCRA.General.ModuleDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModuleDiagramCreator.Test.UnitTests {

    [TestClass]
    public class ModuleDiagramDefinitionsTests {

        [TestMethod]
        public void GraphDefinitions_TestCompletenessWithModuleDefinitions() {
            var moduleDefinitions = McraModuleDefinitions.Instance.ModuleDefinitions;
            var graphDefinitions = ModuleDiagramDefinitions.Instance.GraphDefinitions;

            // Check whether there is a definition for each enum value.
            foreach (var definition in moduleDefinitions) {
                Assert.IsTrue(graphDefinitions.Any(g => g.ActionType == definition.Key), $"Missing graph definition for module '{definition.Key}'. Please add a graph definition for module '{definition.Key}' to file {ModuleDiagramDefinitions._moduleDiagramDefinitionFile}.");
            }
        }
    }
}
