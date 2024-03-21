using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {
    [TestClass]
    public class KineticModelCalculatorFactoryTests {

        /// <summary>
        /// Checks whether the specified dll of each model is available for use.
        /// </summary>
        [TestMethod]
        public void KineticModelCalculatorFactory_TestDll() {
            var modelDefinitions = Enum
                .GetValues(typeof(KineticModelType))
                .Cast<KineticModelType>()
                .Where(r => r != KineticModelType.Undefined)
                .Select(r => MCRAKineticModelDefinitions.Definitions[r.ToString()])
                .ToList();

            foreach (var definition in modelDefinitions) {
                var location = new Uri(Assembly.GetAssembly(typeof(KineticModelCalculatorFactory)).GetName().CodeBase);
                var assemblyFolder = new FileInfo(location.LocalPath).Directory.FullName;
                var dllPath = Path.Combine(assemblyFolder, "Resources", "KineticModels", $"{definition.FileName}");
                Assert.IsTrue(File.Exists(dllPath));
            }
        }
    }
}
