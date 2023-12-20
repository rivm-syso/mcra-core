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
        /// When UseCompleteAnalysedSamples is selected, all individuals that have samples for which one or more substances
        /// are not sampled, should be removed. The table below describes the combinations tested.
        /// NOTES: (1) the -10 values are not tested here, just to simulate some randomized data.
        ///        (2) filtering is done on individual level, and not individual-day; later the day dimension might be included
        ///                       Urine             Blood
        /// Individual-Day      A   B   C           A   D      Include?
        /// -------------------------------------------------------------    
        /// (0, "0")            x   x   -10         x   -       -
        /// (1, "0")            x   x   -           x   x       -
        /// (2, "0")            -   -   x           -   x       -
        /// (3, "0")            x  -10  x           -   x       -
        /// (4, "0")            -   -   -           x   x       -
        /// (5, "0")            x   x   x           x   x       x
        /// (6, "0")            x   x   x          -10  x       x   
        /// 
        /// </summary>
        [TestMethod]
        public void LoadData_SamplesWithSubstancesNotSampled_ShouldExcludeIndividual() {
            var individuals = MockIndividualsGenerator.Create(7, 1, new McraRandomGenerator(), useSamplingWeights: true, codeSurvey: "HumanMonitoringSurvey");
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDaysCodes = individualDays.Select(i => ((IndividualId: i.Individual.Id, i.Day), i)).ToDictionary(k => k.Item1, k => k.i);

            // Create 4 substances:
            // A, B, C      in urine
            // A, D         in blood
            var allSubstances = MockSubstancesGenerator.Create(new string[] { "A", "B", "C", "D" });
            var hbmSurvey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);

            // Blood
            var substancesBlood = allSubstances.Where(s => s.Code == "A" || s.Code == "D").ToList();
            var notSampledBlood = new List<(SimulatedIndividualDay, Compound)> {
                (individualDaysCodes[(0, "0")], substancesBlood[1]),
                (individualDaysCodes[(2, "0")], substancesBlood[0]),
                (individualDaysCodes[(3, "0")], substancesBlood[0])
            };
            var samplingMethodBlood = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood);
            var hbmSamplesBlood = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesBlood, samplingMethodBlood, ConcentrationUnit.ugPerL, null, 1, 0, notSampledBlood);
            hbmSamplesBlood[6].SampleAnalyses.First().Concentrations[allSubstances[0]].Concentration = -10;
            hbmSamplesBlood[6].SampleAnalyses.First().Concentrations[allSubstances[0]].ResTypeString = "MV";

            // Urine
            var substancesUrine = allSubstances.Take(3).ToList();
            var notSampledUrine = new List<(SimulatedIndividualDay, Compound)> {
                (individualDaysCodes[(1, "0")], substancesUrine[2]),
                (individualDaysCodes[(2, "0")], substancesUrine[0]),
                (individualDaysCodes[(2, "0")], substancesUrine[1]),
                (individualDaysCodes[(4, "0")], substancesUrine[0]),
                (individualDaysCodes[(4, "0")], substancesUrine[1]),
                (individualDaysCodes[(4, "0")], substancesUrine[2])
            };  
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);
            var hbmSamplesUrine = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesUrine, samplingMethodUrine, ConcentrationUnit.ugPerL, null, 1, hbmSamplesBlood.Count, notSampledUrine);
            hbmSamplesUrine[0].SampleAnalyses.First().Concentrations[allSubstances[2]].Concentration = -10;
            hbmSamplesUrine[0].SampleAnalyses.First().Concentrations[allSubstances[2]].ResTypeString = "MV";
            hbmSamplesUrine[3].SampleAnalyses.First().Concentrations[allSubstances[1]].Concentration = -10;
            hbmSamplesUrine[3].SampleAnalyses.First().Concentrations[allSubstances[1]].ResTypeString = "MV";

            var allSamples = new List<HumanMonitoringSample>();
            allSamples.AddRange(hbmSamplesBlood);
            allSamples.AddRange(hbmSamplesUrine);

            var compiledData = new CompiledData() {
                AllHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey> { { hbmSurvey.Code, hbmSurvey } },
                AllHumanMonitoringIndividuals = individuals.ToDictionary(c => c.Code),
                AllHumanMonitoringSamples = allSamples.ToDictionary(c => c.Code),
                HumanMonitoringSamplingMethods = allSamples.Select(c => c.SamplingMethod).Distinct().ToList()
            };

            var project = new ProjectDto();
            project.HumanMonitoringSettings.UseCompleteAnalysedSamples = true;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() { AllCompounds = allSubstances };
            var calculator = new HumanMonitoringDataActionCalculator(project);

            calculator.LoadData(data, subsetManager, new CompositeProgressState());

            Assert.IsFalse(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individualDaysCodes[(0, "0")].Individual.Id)));
            Assert.IsFalse(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individualDaysCodes[(1, "0")].Individual.Id)));
            Assert.IsFalse(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individualDaysCodes[(2, "0")].Individual.Id)));
            Assert.IsFalse(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individualDaysCodes[(3, "0")].Individual.Id)));
            Assert.IsFalse(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individualDaysCodes[(4, "0")].Individual.Id)));
            Assert.IsTrue(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individualDaysCodes[(5, "0")].Individual.Id)));
            Assert.IsTrue(data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords.Any(r => r.Individual.Id == individualDaysCodes[(6, "0")].Individual.Id)));
        }
    }
}
