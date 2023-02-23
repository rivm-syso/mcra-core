using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.ActionSettingsManagement {
    [TestClass]
    public class ActionSettingsManagerFactoryTests {

        /// <summary>
        /// Test whether all existing project settings manager classes can be created via
        /// the factory method
        /// </summary>
        [TestMethod]
        public void ActionSettingsManagerFactory_TestFactory() {
            //use reflection to get the types in the module which can be created in the factory
            var asm = typeof(ActionSettingsManagerFactory).Assembly;

            var expectedTypes = asm.GetTypes()
                .Where(t => t.BaseType == typeof(ActionSettingsManagerBase))
                .Select(t => t.Name)
                .ToHashSet();
            var instantiatedTypes = new HashSet<string>();

            foreach (var val in Enum.GetValues(typeof(ActionType)).Cast<ActionType>()) {
                var manager = ActionSettingsManagerFactory.Create(val);
                if (manager != null) {
                    instantiatedTypes.Add(manager.GetType().Name);
                }
            }

            Assert.AreEqual(expectedTypes.Count, instantiatedTypes.Count,
                $"Expected:\n{string.Join("\n", expectedTypes)}\n\nInstantiated:\n{string.Join("\n", instantiatedTypes)}");
        }
    }
}
