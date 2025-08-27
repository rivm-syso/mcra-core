using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.SingleValueConcentrations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.General;

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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllConcentrationSingleValues = FakeConcentrationSingleValuesGenerator.Create(foods, substances, seed: seed),
            };

            var project = new ProjectDto();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData() {
                AllFoods = foods,
                AllCompounds = substances,
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(10);
            var activeSubstances = substances.Skip(5).ToList();
            var measuredSubstances = substances.Take(5).ToList();
            var deterministicSubstanceConversionFactors = FakeDeterministicSubstanceConversionFactorsGenerator
                .Create(measuredSubstances, activeSubstances, random);
            var compiledData = new CompiledData() {
                AllConcentrationSingleValues = FakeConcentrationSingleValuesGenerator.Create(foods, measuredSubstances, seed: seed),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var data = new ActionData() {
                AllFoods = foods,
                AllCompounds = substances,
                ActiveSubstances = activeSubstances,
                ReferenceSubstance = substances.First(),
                DeterministicSubstanceConversionFactors = deterministicSubstanceConversionFactors
            };

            var project = new ProjectDto();
            project.SingleValueConcentrationsSettings.UseDeterministicConversionFactors = true;
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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var substanceSampleCollections = FakeSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                ActiveSubstanceSampleCollections = substanceSampleCollections,
                SingleValueConcentrationUnit = ConcentrationUnit.mgPerKg
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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var mrls = FakeMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(10);
            var activeSubstances = substances.Skip(5).ToList();
            var measuredSubstances = substances.Take(5).ToList();
            var substanceSampleCollections = FakeSampleCompoundCollectionsGenerator
                .Create(foods, activeSubstances, random);
            var deterministicSubstanceConversionFactors = FakeDeterministicSubstanceConversionFactorsGenerator
                .Create(measuredSubstances, activeSubstances, random);
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = activeSubstances,
                ReferenceSubstance = substances.First(),
                ActiveSubstanceSampleCollections = substanceSampleCollections,
                DeterministicSubstanceConversionFactors = deterministicSubstanceConversionFactors
            };
            var project = new ProjectDto();
            project.SingleValueConcentrationsSettings.UseDeterministicConversionFactors = true;
            var calculator = new SingleValueConcentrationsActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeWithConversion");
        }
    }
}
