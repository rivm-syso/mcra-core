using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.OccurrencePatterns;
using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the AgriculturalUses action
    /// </summary>
    [TestClass]
    public class OccurrencePatternsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the OccurrencePatterns action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var foods = MockFoodsGenerator.Create(2);
            var agriculturalUses = new List<OccurrencePattern> {
                new OccurrencePattern() {
                    Code = "AU1",
                    Compounds = substances,
                    Food = foods[0],
                    StartDate = new DateTime(),
                    EndDate = new DateTime(),
                    Location = "Location1",
                    OccurrenceFraction = .8,
                },
                new OccurrencePattern() {
                    Code = "AU2",
                    Compounds = substances,
                    Food = foods[1],
                    StartDate = new DateTime(),
                    EndDate = new DateTime(),
                    Location = "Location2",
                    OccurrenceFraction = .8,
                }
            };

            var compiledData = new CompiledData() {
                AllOccurrencePatterns = agriculturalUses,
            };
            var sampleOriginInfos = new Dictionary<Food, List<ISampleOrigin>> {
                [foods[0]] = new List<ISampleOrigin> {
                    new SampleOriginRecord { Food = foods[0], Location = "NL", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = foods[0], Location = null, Fraction = 0F, NumberOfSamples = 0 }
                },
                [foods[1]] = new List<ISampleOrigin> {
                    new SampleOriginRecord { Food = foods[1], Location = "NL", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = foods[1], Location = null, Fraction = 0F, NumberOfSamples = 0 }
                }
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData {
                AllFoods = foods,
                SampleOriginInfos = sampleOriginInfos
            };
            var calculator = new OccurrencePatternsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsNotNull(data.MarginalOccurrencePatterns);
            Assert.AreEqual(2, data.MarginalOccurrencePatterns.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
        }

        /// <summary>
        /// Runs the OccurrencePatterns module as compute
        /// config.RecomputeOccurrencePatterns = true;
        /// config.ScaleUpOccurencePatterns = true;
        /// config.RestrictOccurencePatternScalingToAuthorisedUses = true;
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsActionCalculator_TestCompute1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var foods = MockFoodsGenerator.Create(2);
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator
                .Create(
                    foods,
                    substances,
                    random
                );

            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstanceSampleCollections = activeSubstanceSampleCollections
            };

            var project = new ProjectDto();
            var config = project.GetModuleConfiguration<OccurrencePatternsModuleConfig>();
            config.RecomputeOccurrencePatterns = true;
            config.ScaleUpOccurencePatterns = true;
            config.RestrictOccurencePatternScalingToAuthorisedUses = true;

            var calculator = new OccurrencePatternsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "AgriculturalUse1");
            Assert.IsNotNull(data.MarginalOccurrencePatterns);
            Assert.AreEqual(2, data.MarginalOccurrencePatterns.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Runs the OccurrencePatterns module as compute
        /// config.RecomputeOccurrencePatterns = true;
        /// config.ScaleUpOccurencePatterns = false;
        /// config.RestrictOccurencePatternScalingToAuthorisedUses = false;
        /// </summary>
        [TestMethod]
        public void OccurrencePatternsActionCalculator_TestCompute2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var foods = MockFoodsGenerator.Create(2);
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator
                .Create(
                    foods,
                    substances,
                    random
                );

            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstanceSampleCollections = activeSubstanceSampleCollections
            };

            var project = new ProjectDto();
            var config = project.GetModuleConfiguration<OccurrencePatternsModuleConfig>();
            config.RecomputeOccurrencePatterns = true;
            config.ScaleUpOccurencePatterns = false;
            config.RestrictOccurencePatternScalingToAuthorisedUses = false;

            var calculator = new OccurrencePatternsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "AgriculturalUse2");
            Assert.IsNotNull(data.MarginalOccurrencePatterns);
            Assert.AreEqual(2, data.MarginalOccurrencePatterns.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }
    }
}