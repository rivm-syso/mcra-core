using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
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
            var humanMonitoringSurvey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);
            var hbmSamples = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substances, samplingMethod);

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
            Assert.IsNotNull(data.HbmAllSamples);
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
            var hbmSurvey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);

            var hbmSamples = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL);

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

        /// <summary>
        /// 
        ///       Urine             Blood
        ///     A   B   C           A   D      Include?
        /// ----------------------------------------------    
        /// 1   x   x   -10         x   -       -
        /// 2   x   x   -           x   x       -
        /// 3   x  -10  x           -   x       -
        /// 4   -   -   -           x   x       -
        /// 5   x   x   x           x   x       x
        /// 6   x   x   x           -10 x       x   
        /// 
        /// </summary>
        [TestMethod]
        [Ignore]    // TODO: Tij. 2023-11-30. Finish unit test, test different scenarios
        //       individual     day   substance notSampled      missingValue
        [DataRow(0, "0", BiologicalMatrix.Blood, "CMP3", true, false)]

        public void CompleteCases_NotSampledInOneMatrix_IndividualShouldBeExcluded(int individual, string day, BiologicalMatrix biologicalMatrix, string substanceCode, bool notSampled, bool missingValue) {
            var individuals = MockIndividualsGenerator.Create(3, 2, new McraRandomGenerator(), useSamplingWeights: true, codeSurvey: "HumanMonitoringSurvey");
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            // Create 4 substances:
            // A, B, C      in urine
            // A, D         in blood
            var allSubstances = MockSubstancesGenerator.Create(4);
            var substancesBlood = allSubstances.Where(s => s.Code == "CMP0" || s.Code == "CMP3").ToList();
            var substancesUrine = allSubstances.Take(3).ToList();

            var hbmSurvey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);

            var substanceNotSampled = allSubstances.Find(s => s.Code == substanceCode);
            var individualDay = individualDays.FirstOrDefault(id => id.Individual.Id == individual && id.Day == day);
            var samplingMethodBlood = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood);
            var notSampledCompounds = new Dictionary<SimulatedIndividualDay, Compound> {
                { individualDay, substanceNotSampled}
            };
            var hbmSamplesBlood = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesBlood, samplingMethodBlood, ConcentrationUnit.ugPerL, null, 1, 0, notSampledCompounds);

            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);
            var hbmSamplesUrine = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesUrine, samplingMethodUrine, ConcentrationUnit.ugPerL, null, 1, hbmSamplesBlood.Count);

            var allSamples = new List<HumanMonitoringSample>();
            allSamples.AddRange(hbmSamplesBlood);
            allSamples.AddRange(hbmSamplesUrine);

            var compiledData = new CompiledData() {
                AllHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey> { { hbmSurvey.Code, hbmSurvey } },
                AllHumanMonitoringIndividuals = individuals.ToDictionary(c => c.Code),
                AllHumanMonitoringSamples = allSamples.ToDictionary(c => c.Code),
                HumanMonitoringSamplingMethods = allSamples.Select(c => c.SamplingMethod).ToList()
            };

            var project = new ProjectDto();
            project.HumanMonitoringSettings.UseCompleteAnalysedSamples = true;


            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                AllCompounds = allSubstances,
            };
            var calculator = new HumanMonitoringDataActionCalculator(project);

            calculator.LoadData(data, subsetManager, new CompositeProgressState());


            Assert.IsFalse(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individual && r.Day == day)));

            foreach (var collection in data.HbmSampleSubstanceCollections) {
                Assert.AreEqual(ConcentrationUnit.ugPerL, collection.ConcentrationUnit);
            }
        }
    }
}
