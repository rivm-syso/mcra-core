using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
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
                var assemblyFolder = (new FileInfo(location.LocalPath).Directory).FullName;
                var dllPath = Path.Combine(assemblyFolder, $"Resources/KineticModels/{definition.DllName}.dll").Replace(@"\", "/");
                Assert.IsTrue(File.Exists(dllPath));
            }
        }
    }
}
