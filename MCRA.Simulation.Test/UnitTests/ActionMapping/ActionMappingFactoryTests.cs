using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

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
                .Where(r => r != ActionType.Unknown);
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