using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {
    [TestClass]
    public class KineticModelCalculatorFactoryTests {

        /// <summary>
        /// Checks whether the deSolve implementation of all deSolve models is available.
        /// </summary>
        [TestMethod]
        public void KineticModelCalculatorFactory_TestDeSolveImplementationsAvailable() {
            var modelDefinitions = MCRAKineticModelDefinitions.Definitions.Values
                .Where(r => r.Format != KineticModelType.SBML)
                .ToList();

            foreach (var definition in modelDefinitions) {
                var location = Assembly.GetAssembly(typeof(KineticModelCalculatorFactory)).Location;
                var assemblyFolder = new FileInfo(location).Directory.FullName;
                var dllPath = Path.Combine(assemblyFolder, "Resources", "KineticModels", $"{definition.FileName}");
                Assert.IsTrue(File.Exists(dllPath));
            }
        }
    }
}
