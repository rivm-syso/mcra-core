using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.SingleValueConcentrations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the SingleValueConcentrationsAction action
    /// </summary>
    [TestClass]
    public class SingleValueConcentrationsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the single value concentrations module as data action.
        /// </summary>
        [TestMethod]
        public void SingleValueConcentrationsActionCalculator_TestLoad() {
            int seed = 1;
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllConcentrationSingleValues = MockConcentrationSingleValuesGenerator.Create(foods, substances, seed: seed),
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                AllFoods = foods,
                AllCompounds = substances,
                ActiveSubstances = substances,
                ReferenceCompound = substances.First(),
            };
            var calculator = new SingleValueConcentrationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
        }


        /// <summary>
        /// Runs the single value concentrations module as data action with substance conversion.
        /// </summary>
        [TestMethod]
        public void SingleValueConcentrationsActionCalculator_TestLoadWithConversion() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(10);
            var activeSubstances = substances.Skip(5).ToList();
            var measuredSubstances = substances.Take(5).ToList();
            var deterministicSubstanceConversionFactors = MockDeterministicSubstanceConversionFactorsGenerator
                .Create(measuredSubstances, activeSubstances, random);
            var compiledData = new CompiledData() {
                AllConcentrationSingleValues = MockConcentrationSingleValuesGenerator.Create(foods, measuredSubstances, seed: seed),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var data = new ActionData() {
                AllFoods = foods,
                AllCompounds = substances,
                ActiveSubstances = activeSubstances,
                ReferenceCompound = substances.First(),
                DeterministicSubstanceConversionFactors = deterministicSubstanceConversionFactors
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.UseDeterministicConversionFactors = true;
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new SingleValueConcentrationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadWithConversion");
        }

        /// <summary>
        /// Runs the single value concentrations action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueConcentrationsActionCalculator_TestComputeFromSamples() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var substanceSampleCollections = MockSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = substances,
                ReferenceCompound = substances.First(),
                ActiveSubstanceSampleCollections = substanceSampleCollections
            };
            var project = new ProjectDto();
            var calculator = new SingleValueConcentrationsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeFromSamples");
        }

        /// <summary>
        /// Runs the single value concentrations action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueConcentrationsActionCalculator_TestComputeFromMrl() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var mrls = MockMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = substances,
                ReferenceCompound = substances.First(),
                MaximumConcentrationLimits = mrls
            };
            var project = new ProjectDto();
            var calculator = new SingleValueConcentrationsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeFromMrl");
        }

        /// <summary>
        /// Runs the single value concentrations action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueConcentrationsActionCalculator_TestComputeWithConversion() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(10);
            var activeSubstances = substances.Skip(5).ToList();
            var measuredSubstances = substances.Take(5).ToList();
            var substanceSampleCollections = MockSampleCompoundCollectionsGenerator
                .Create(foods, activeSubstances, random);
            var deterministicSubstanceConversionFactors = MockDeterministicSubstanceConversionFactorsGenerator
                .Create(measuredSubstances, activeSubstances, random);
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = activeSubstances,
                ReferenceCompound = substances.First(),
                ActiveSubstanceSampleCollections = substanceSampleCollections,
                DeterministicSubstanceConversionFactors = deterministicSubstanceConversionFactors
            };
            var project = new ProjectDto();
            project.ConcentrationModelSettings.UseDeterministicConversionFactors = true;
            var calculator = new SingleValueConcentrationsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeWithConversion");
        }
    }
}
