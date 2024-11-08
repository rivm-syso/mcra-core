using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.KineticConversionFactors;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the KineticConversionFactors action
    /// </summary>
    [TestClass]
    public class KineticConversionFactorsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the KineticConversionFactors action: load data, load default data, summarize
        /// action result, load data uncertain method
        /// </summary>
        [TestMethod]
        public void KineticConversionFactorActionCalculator_TestLoadKineticModelInstance() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var substances = FakeSubstancesGenerator.Create(1);
            var referenceCompound = substances.First();
            var kineticModelinstance = FakeKineticModelsGenerator.CreatePbkModelInstance(referenceCompound);
            var kineticModelInstances = new List<KineticModelInstance>() { kineticModelinstance };

            var compiledData = new CompiledData() {
                AllKineticModelInstances = kineticModelInstances.ToList(),
            };

            var project = new ProjectDto();
            var config = project.KineticModelsSettings;
            config.ExposureRoutes = [ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation];
            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = referenceCompound,
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var calculator = new KineticConversionFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = new[] { UncertaintySource.KineticConversionFactors }
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.KineticConversionFactors] = random
            };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }
    }
}