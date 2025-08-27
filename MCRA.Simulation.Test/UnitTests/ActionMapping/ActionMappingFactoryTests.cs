using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Test.UnitTests {

    /// <summary>
    /// ActionMappingFactoryTests
    /// </summary>
    [TestClass()]
    public class ActionMappingFactoryTests {

        /// <summary>
        /// Maps all actions in a new project.
        /// </summary>
        [TestMethod()]
        public void ActionMappingFactory_CreateAllActionTypesTest() {
            var enumValues = Enum.GetValues(typeof(ActionType))
                .Cast<ActionType>()
                .Where(r => (int)r >= 0);
            foreach (var value in enumValues) {
                var project = new ProjectDto() {
                    ActionType = value,
                };
                var mapping = ActionMappingFactory.Create(project, value);
                Assert.AreEqual(project, mapping.Project);
                Assert.AreEqual(project.ActionType, mapping.MainActionType);
                Assert.IsNotNull(mapping.GetTableGroupMappings().Any());
            }
        }
    }
}