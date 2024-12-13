using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.SingleValueDietaryExposures;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Tests the single value dietary exposures module.
    /// </summary>
    [TestClass]
    public class SingleValueDietaryExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the single value dietary exposures action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueDietaryExposuresActionCalculator_TestComputeChronic() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            testChronicCalculationMethod(SingleValueDietaryExposuresCalculationMethod.IEDI, random);
            testChronicCalculationMethod(SingleValueDietaryExposuresCalculationMethod.TMDI, random);
            testChronicCalculationMethod(SingleValueDietaryExposuresCalculationMethod.NEDI1, random);
            testChronicCalculationMethod(SingleValueDietaryExposuresCalculationMethod.NEDI2, random);
        }

        /// <summary>
        /// Runs the single value dietary exposures action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueDietaryExposuresActionCalculator_TestComputeChronicProcessing() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            testChronicCalculationMethodProcessing(SingleValueDietaryExposuresCalculationMethod.IEDI, random);
            testChronicCalculationMethodProcessing(SingleValueDietaryExposuresCalculationMethod.TMDI, random);
            testChronicCalculationMethodProcessing(SingleValueDietaryExposuresCalculationMethod.NEDI1, random);
            testChronicCalculationMethodProcessing(SingleValueDietaryExposuresCalculationMethod.NEDI2, random);
        }

        /// <summary>
        /// Runs the single value dietary exposures action as compute.
        /// </summary>
        [TestMethod]
        public void SingleValueDietaryExposuresActionCalculator_TestComputeAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            testAcuteCalculationMethod(SingleValueDietaryExposuresCalculationMethod.IESTI, random);
            testAcuteCalculationMethod(SingleValueDietaryExposuresCalculationMethod.IESTINew, random);
        }

        private void testChronicCalculationMethod(
            SingleValueDietaryExposuresCalculationMethod method,
            IRandom random
        ) {
            var foods = FakeFoodsGenerator.CreateFoodsWithUnitWeights(15, random, fractionMissing: .1);
            var substances = FakeSubstancesGenerator.Create(3);
            var singleValueConsumptionModels = FakeSingleValueConsumptionModelsGenerator
                .Create(foods, random);
            var singleValueConcentrationModels = FakeSingleValueConcentrationModelsGenerator
                .Create(foods, substances, random);
            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                SingleValueConsumptionModels = singleValueConsumptionModels,
                ActiveSubstanceSingleValueConcentrations = singleValueConcentrationModels,
                SingleValueConsumptionIntakeUnit = ConsumptionIntakeUnit.gPerDay,
                SingleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg,
            };
            var project = new ProjectDto();
            project.SingleValueDietaryExposuresSettings.SingleValueDietaryExposureCalculationMethod = method;
            var calculator = new SingleValueDietaryExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, $"TestCompute{method.GetShortDisplayName()}");
        }

        private void testAcuteCalculationMethod(
          SingleValueDietaryExposuresCalculationMethod method,
          IRandom random
        ) {
            var baseFoods = FakeFoodsGenerator.CreateFoodsWithUnitWeights(5, random, fractionMissing: .1);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(baseFoods, processingTypes);
            var foods = baseFoods.Concat(processedFoods).ToList();
            var substances = FakeSubstancesGenerator.Create(3);
            var processingFactors = FakeProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(foods, substances, processingTypes, random, fractionMissing: .1);
            var singleValueConsumptionModels = FakeSingleValueConsumptionModelsGenerator
                .Create(foods, random);
            var singleValueConcentrationModels = FakeSingleValueConcentrationModelsGenerator
                .Create(baseFoods, substances, random);
            var unitVariabilityFactors = FakeUnitVariabilityFactorsGenerator
                .Create(baseFoods, substances, random, 0.1);

            var iestiSpecialCases = new List<IestiSpecialCase>() {new(){
                Food = foods.First(),
                Substance = substances.First(),
                ApplicationType = HarvestApplicationType.PostHarvest,
                }
            };

            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                SingleValueConsumptionModels = singleValueConsumptionModels,
                ActiveSubstanceSingleValueConcentrations = singleValueConcentrationModels,
                UnitVariabilityDictionary = unitVariabilityFactors,
                ProcessingFactorModels = processingFactors,
                SingleValueConsumptionIntakeUnit = ConsumptionIntakeUnit.gPerDay,
                SingleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg,
                IestiSpecialCases = iestiSpecialCases,
            };
            var project = new ProjectDto();
            project.SingleValueDietaryExposuresSettings.SingleValueDietaryExposureCalculationMethod = method;
            var calculator = new SingleValueDietaryExposuresActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestCompute-{method.GetShortDisplayName()}");
        }

        private void testChronicCalculationMethodProcessing(
            SingleValueDietaryExposuresCalculationMethod method,
            IRandom random
        ) {
            var baseFoods = FakeFoodsGenerator.CreateFoodsWithUnitWeights(5, random, fractionMissing: .1);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(baseFoods, processingTypes);
            var foods = baseFoods.Concat(processedFoods).ToList();

            var substances = FakeSubstancesGenerator.Create(3);
            var processingFactors = FakeProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(foods, substances, processingTypes, random, fractionMissing: .1);
            var singleValueConsumptionModels = FakeSingleValueConsumptionModelsGenerator
                .Create(foods, random);
            var singleValueConcentrationModels = FakeSingleValueConcentrationModelsGenerator
                .Create(foods, substances, random);
            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                SingleValueConsumptionModels = singleValueConsumptionModels,
                ActiveSubstanceSingleValueConcentrations = singleValueConcentrationModels,
                ProcessingFactorModels = processingFactors,
                SingleValueConsumptionIntakeUnit = ConsumptionIntakeUnit.gPerDay,
                SingleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg,
            };
            var project = new ProjectDto();
            project.SingleValueDietaryExposuresSettings.SingleValueDietaryExposureCalculationMethod = method;
            var calculator = new SingleValueDietaryExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, $"TestCompute{method.GetShortDisplayName()}-Processing");
        }
    }
}
