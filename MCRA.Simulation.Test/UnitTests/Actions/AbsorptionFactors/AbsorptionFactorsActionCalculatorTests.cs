using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.KineticModels;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the KineticModels action
    /// </summary>
    [TestClass]
    public class AbsorptionFactorsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the KineticModels action: load data, load default data, summarize
        /// action result, load data uncertain method
        /// </summary>
        [TestMethod]
        public void KineticModelsActionCalculator_TestLoadAbsorptionFactors() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var routes = new[] {
                ExposureRoute.Dermal,
                ExposureRoute.Inhalation,
                ExposureRoute.Oral
            };
            var substances = FakeSubstancesGenerator.Create(1);
            var referenceCompound = substances.First();
            var simpleAbsorptionFactors = FakeAbsorptionFactorsGenerator.Create(routes, substances);

            var compiledData = new CompiledData() {
                AllAbsorptionFactors = simpleAbsorptionFactors.ToList(),
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

            var calculator = new KineticModelsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsTrue(data.AbsorptionFactors.Any());

            var factorialSet = new UncertaintyFactorialSet() {
                //UncertaintySources = new List<UncertaintySource>() { UncertaintySource.KineticModelParameters }
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                //[UncertaintySource.KineticModelParameters] = random
            };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Runs the KineticModels action: load data, load default data, summarize
        /// action result, load data uncertain method
        /// </summary>
        [TestMethod]
        public void KineticModelsActionCalculator_TestLoadKineticModelInstance() {
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

            var calculator = new KineticModelsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.AreEqual(3, data.AbsorptionFactors.Count);
            Assert.IsNotNull(data.AbsorptionFactors);
            var factorialSet = new UncertaintyFactorialSet() { };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> { };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }
    }
}