using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.HumanMonitoringData;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the HumanMonitoringData action
    /// </summary>
    [TestClass]
    public class HumanMonitoringDataActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the HumanMonitoringData action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void HumanMonitoringDataActionCalculator_TestLoadData() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator
                .Create(25, 2, random, useSamplingWeights: true, codeSurvey: "HumanMonitoringSurvey");
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var humanMonitoringSurvey = FakeHbmDataGenerator.MockHumanMonitoringSurvey(individualDays);
            var hbmSamples = FakeHbmDataGenerator.MockHumanMonitoringSamples(individualDays, substances, samplingMethod);

            var compiledData = new CompiledData() {
                AllHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey> { { humanMonitoringSurvey.Code, humanMonitoringSurvey } },
                AllHumanMonitoringIndividuals = individuals.ToDictionary(c => c.Code),
                AllHumanMonitoringSamples = hbmSamples.ToDictionary(c => c.Code),
                HumanMonitoringSamplingMethods = hbmSamples.Select(c => c.SamplingMethod).ToList()
            };

            var project = new ProjectDto();
            project.HumanMonitoringSettings.SamplingMethodCodes = new List<string>() { samplingMethod.Code };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                AllCompounds = substances,
            };
            var calculator = new HumanMonitoringDataActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsNotNull(data.HbmSurveys);
            Assert.IsNotNull(data.HbmSamples);
            Assert.IsNotNull(data.HbmSamplingMethods);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.RPFs);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
        }

        [TestMethod]
        public void HumanMonitoringDataActionCalculator_HbmUnit_ShouldBeReadFromAnalyticalMethodCompound() {
            var individuals = MockIndividualsGenerator.Create(25, 2, new McraRandomGenerator(), useSamplingWeights: true, codeSurvey: "HumanMonitoringSurvey");
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSurvey = FakeHbmDataGenerator.MockHumanMonitoringSurvey(individualDays);

            var hbmSamples = FakeHbmDataGenerator.MockHumanMonitoringSamples(individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL);

            var compiledData = new CompiledData() {
                AllHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey> { { hbmSurvey.Code, hbmSurvey } },
                AllHumanMonitoringIndividuals = individuals.ToDictionary(c => c.Code),
                AllHumanMonitoringSamples = hbmSamples.ToDictionary(c => c.Code),
                HumanMonitoringSamplingMethods = hbmSamples.Select(c => c.SamplingMethod).ToList()
            };

            var project = new ProjectDto();

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                AllCompounds = substances,
            };
            var calculator = new HumanMonitoringDataActionCalculator(project);

            calculator.LoadData(data, subsetManager, new CompositeProgressState());

            foreach (var collection in data.HbmSampleSubstanceCollections) {
                Assert.AreEqual(ConcentrationUnit.ugPerL, collection.ConcentrationUnit);
            }
        }
    }
}
