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
        /// When UseCompleteAnalysedSamples is selected, samples of individual-days for which one or more substances
        /// are not measured by a sampling method, should be removed. The table below describes the combinations tested.
        /// NOTES: (1) the -10 values are not tested here, just to simulate some random missing value data.
        ///        (2) day "1" has not sampled individual 6, but this should not affect other samples.
        ///        (3) for individual day (5, "1") blood has not been sampled at all.
        ///        
        ///                                   Urine             Blood                  
        /// SampleId    Individual-Day      A   B   C           A   D       Include?    
        /// --------------------------------------------------------------------------
        ///     0         (0, "0")          x   x   -10         x   -    |    -
        ///     1         (1, "0")          x   x   -           x   x    |    -
        ///     2         (2, "0")          -   -   x           -   x    |    -
        ///     3         (3, "0")          x  -10  x           -   x    |    -
        ///     4         (4, "0")          -   -   -           x   x    |    -
        ///     5         (5, "0")          x   x   x           -10 x    |    x
        ///     6         (6, "0")          x   x   x           x   x    |    x
        /// 
        ///     7         (0, "1")          x   x   -           x   x    |    -
        ///     8         (1, "1")          x   x   x           -   -    |    -
        ///     9         (2, "1")          -   x   x           x   x    |    -
        ///     10        (3, "1")          x  -10  x           -10 x    |    x
        ///     11        (4, "1")          x   x   x           x   x    |    x
        ///     12        (5, "1")          -10 x   x           (n.a.)   |    -
        ///     
        /// n.a. = not available, no sample is measured for blood for individual 5 on day 1.
        /// </summary>
        [TestMethod]
        public void HumanMonitoringDataActionCalculator_SamplesWithSubstancesNotSampledIndividualDays_ShouldExcludeIndividual() {
            var individuals = MockIndividualsGenerator.Create(7, 2, new McraRandomGenerator(), useSamplingWeights: true, codeSurvey: "HumanMonitoringSurvey");
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            individualDays.RemoveAt(13);  // Day 1 has not sampled individual 6.
            var individualDaysCodes = individualDays.Select(i => ((IndividualId: i.Individual.Id, i.Day), i)).ToDictionary(k => k.Item1, k => k.i);

            // Create 4 substances:
            // A, B, C      in urine
            // A, D         in blood
            var allSubstances = MockSubstancesGenerator.Create(new string[] { "A", "B", "C", "D" });
            var hbmSurvey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);

            // Urine
            var substancesUrine = allSubstances.Take(3).ToList();
            var notSampledUrine = new List<(SimulatedIndividualDay, Compound)> {
                (individualDaysCodes[(1, "0")], substancesUrine[2]),
                (individualDaysCodes[(2, "0")], substancesUrine[0]),
                (individualDaysCodes[(2, "0")], substancesUrine[1]),
                (individualDaysCodes[(4, "0")], substancesUrine[0]),
                (individualDaysCodes[(4, "0")], substancesUrine[1]),
                (individualDaysCodes[(4, "0")], substancesUrine[2]),
                (individualDaysCodes[(0, "1")], substancesUrine[2]),
                (individualDaysCodes[(2, "1")], substancesUrine[0])
            };
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);
            var hbmSamplesUrine = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesUrine, samplingMethodUrine, ConcentrationUnit.ugPerL, null, 1, 0, notSampledUrine);
            hbmSamplesUrine = hbmSamplesUrine.OrderBy(s => s.DayOfSurvey).ToList();
            hbmSamplesUrine[0].SampleAnalyses.First().Concentrations[allSubstances[2]].Concentration = -10;
            hbmSamplesUrine[0].SampleAnalyses.First().Concentrations[allSubstances[2]].ResTypeString = "MV";
            hbmSamplesUrine[3].SampleAnalyses.First().Concentrations[allSubstances[1]].Concentration = -10;
            hbmSamplesUrine[3].SampleAnalyses.First().Concentrations[allSubstances[1]].ResTypeString = "MV";
            hbmSamplesUrine[10].SampleAnalyses.First().Concentrations[allSubstances[1]].Concentration = -10;
            hbmSamplesUrine[10].SampleAnalyses.First().Concentrations[allSubstances[1]].ResTypeString = "MV";
            hbmSamplesUrine[12].SampleAnalyses.First().Concentrations[allSubstances[0]].Concentration = -10;
            hbmSamplesUrine[12].SampleAnalyses.First().Concentrations[allSubstances[0]].ResTypeString = "MV";

            // Blood
            var substancesBlood = allSubstances.Where(s => s.Code == "A" || s.Code == "D").ToList();
            var notSampledBlood = new List<(SimulatedIndividualDay, Compound)> {
                (individualDaysCodes[(0, "0")], substancesBlood[1]),
                (individualDaysCodes[(2, "0")], substancesBlood[0]),
                (individualDaysCodes[(3, "0")], substancesBlood[0]),
                (individualDaysCodes[(1, "1")], substancesBlood[0]),
                (individualDaysCodes[(1, "1")], substancesBlood[1]),
            };
            var samplingMethodBlood = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood);
            var hbmSamplesBlood = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesBlood, samplingMethodBlood, ConcentrationUnit.ugPerL, null, 1, hbmSamplesUrine.Count, notSampledBlood);
            hbmSamplesBlood = hbmSamplesBlood.OrderBy(s => s.DayOfSurvey).ToList();
            hbmSamplesBlood[5].SampleAnalyses.First().Concentrations[allSubstances[0]].Concentration = -10;
            hbmSamplesBlood[5].SampleAnalyses.First().Concentrations[allSubstances[0]].ResTypeString = "MV";
            hbmSamplesBlood[10].SampleAnalyses.First().Concentrations[allSubstances[0]].Concentration = -10;
            hbmSamplesBlood[10].SampleAnalyses.First().Concentrations[allSubstances[0]].ResTypeString = "MV";
            hbmSamplesBlood.Remove(hbmSamplesBlood.FirstOrDefault(s => s.Individual.Id == 5 && s.DayOfSurvey == "1"));

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

            var sampleIsPresentForIndividualDay = (int IndividualId, string Day) => data.HbmSampleSubstanceCollections.Any(c => c.HumanMonitoringSampleSubstanceRecords
                .Any(r => r.Individual.Id == individualDaysCodes[(IndividualId, Day)].Individual.Id && r.Day == individualDaysCodes[(IndividualId, Day)].Day));

            Assert.IsFalse(sampleIsPresentForIndividualDay(0, "0"));
            Assert.IsFalse(sampleIsPresentForIndividualDay(1, "0"));
            Assert.IsFalse(sampleIsPresentForIndividualDay(2, "0"));
            Assert.IsFalse(sampleIsPresentForIndividualDay(3, "0"));
            Assert.IsFalse(sampleIsPresentForIndividualDay(4, "0"));
            Assert.IsTrue(sampleIsPresentForIndividualDay(5, "0"));
            Assert.IsTrue(sampleIsPresentForIndividualDay(6, "0"));

            Assert.IsFalse(sampleIsPresentForIndividualDay(0, "1"));
            Assert.IsFalse(sampleIsPresentForIndividualDay(1, "1"));
            Assert.IsFalse(sampleIsPresentForIndividualDay(2, "1"));
            Assert.IsTrue(sampleIsPresentForIndividualDay(3, "1"));
            Assert.IsTrue(sampleIsPresentForIndividualDay(4, "1"));
            Assert.IsFalse(sampleIsPresentForIndividualDay(5, "1"));

            var hbmSampleSubstanceCollections = data.HbmSampleSubstanceCollections.ToList();
            Assert.AreEqual(hbmSampleSubstanceCollections[0].HumanMonitoringSampleSubstanceRecords.Count, hbmSampleSubstanceCollections[1].HumanMonitoringSampleSubstanceRecords.Count);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void HumanMonitoringDataActionCalculator_FilterOnSelectedTimePoints_ShouldYieldSamplesWithSelectedTimepoints(bool filterRepeatedMeasurements) {
            var individuals = MockIndividualsGenerator.Create(25, 2, new McraRandomGenerator(), useSamplingWeights: true, codeSurvey: "HumanMonitoringSurvey");
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSamples = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL);
            var hbmSurvey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);
            var compiledData = new CompiledData() {
                AllHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey> { { hbmSurvey.Code, hbmSurvey } },
                AllHumanMonitoringIndividuals = individuals.ToDictionary(c => c.Code),
                AllHumanMonitoringSamples = hbmSamples.ToDictionary(c => c.Code),
                HumanMonitoringSamplingMethods = hbmSamples.Select(c => c.SamplingMethod).ToList()
            };

            var timepoints = filterRepeatedMeasurements
                ? new List<string> { hbmSurvey.Timepoints.First().Code }
                : hbmSurvey.Timepoints.Select(t => t.Code).ToList();

            var project = new ProjectDto();
            project.HumanMonitoringSettings.FilterRepeatedMeasurements = filterRepeatedMeasurements;
            project.HumanMonitoringSettings.RepeatedMeasurementTimepointCodes = timepoints;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                AllCompounds = substances,
            };
            var calculator = new HumanMonitoringDataActionCalculator(project);

            calculator.LoadData(data, subsetManager, new CompositeProgressState());

            foreach (var collection in data.HbmSampleSubstanceCollections) {
                Assert.IsTrue(collection.HumanMonitoringSampleSubstanceRecords.All(r => timepoints.Contains(r.Day)));
            }
        }

    }
}
