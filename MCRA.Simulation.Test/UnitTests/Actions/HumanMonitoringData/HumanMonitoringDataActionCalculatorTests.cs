using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
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
            var humanMonitoringSurveys = FakeHbmDataGenerator.MockHumanMonitoringSurveys(individualDays);
            var hbmSamples = FakeHbmDataGenerator.MockHumanMonitoringSamples(individualDays, substances, samplingMethod);

            var compiledData = new CompiledData() {
                AllHumanMonitoringSurveys = humanMonitoringSurveys.ToDictionary(c => c.Code, c => c),
                AllHumanMonitoringIndividuals = individuals.ToDictionary(c => c.Code, c => c),
                AllHumanMonitoringSamples = hbmSamples.ToDictionary(c => c.Code, c => c),
                HumanMonitoringSamplingMethods = hbmSamples.Select(c => c.SamplingMethod).ToList()
            };

            var project = new ProjectDto();
            project.HumanMonitoringSettings.SurveyCodes = humanMonitoringSurveys.Select(c => c.Code).ToList();
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
            var humanMonitoringSurveys = FakeHbmDataGenerator.MockHumanMonitoringSurveys(individualDays);

            var unitStringAnalyticalMethodCompound = "µg/L";
            var hbmSamples = FakeHbmDataGenerator.MockHumanMonitoringSamples(individualDays, substances, samplingMethod, unitStringAnalyticalMethodCompound);

            var compiledData = new CompiledData() {
                AllHumanMonitoringSurveys = humanMonitoringSurveys.ToDictionary(c => c.Code, c => c),
                AllHumanMonitoringIndividuals = individuals.ToDictionary(c => c.Code, c => c),
                AllHumanMonitoringSamples = hbmSamples.ToDictionary(c => c.Code, c => c),
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

            Assert.AreEqual(ConcentrationUnitConverter.FromString(unitStringAnalyticalMethodCompound), data.HbmConcentrationUnit);
        }
    }
}
