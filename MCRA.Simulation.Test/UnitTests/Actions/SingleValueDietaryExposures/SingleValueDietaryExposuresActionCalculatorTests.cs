using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.SingleValueDietaryExposures;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var foods = MockFoodsGenerator.CreateFoodsWithUnitWeights(15, random, fractionMissing: .1);
            var substances = MockSubstancesGenerator.Create(3);
            var singleValueConsumptionModels = MockSingleValueConsumptionModelsGenerator
                .Create(foods, random);
            var singleValueConcentrationModels = MockSingleValueConcentrationModelsGenerator
                .Create(foods, substances, random);
            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstances = substances,
                ReferenceCompound = substances.First(),
                SingleValueConsumptionModels = singleValueConsumptionModels,
                ActiveSubstanceSingleValueConcentrations = singleValueConcentrationModels,
                SingleValueConsumptionIntakeUnit = ConsumptionIntakeUnit.gPerDay,
                SingleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg,
            };
            var project = new ProjectDto();
            project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod = method;
            var calculator = new SingleValueDietaryExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, $"TestCompute{method.GetShortDisplayName()}");
        }

        private void testAcuteCalculationMethod(
          SingleValueDietaryExposuresCalculationMethod method,
          IRandom random
        ) {
            var baseFoods = MockFoodsGenerator.CreateFoodsWithUnitWeights(5, random, fractionMissing: .1);
            var processingTypes = MockProcessingTypesGenerator.Create(3);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(baseFoods, processingTypes);
            var foods = baseFoods.Concat(processedFoods).ToList();
            var substances = MockSubstancesGenerator.Create(3);
            var processingFactors = MockProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(foods, substances, processingTypes, random, fractionMissing: .1);
            var singleValueConsumptionModels = MockSingleValueConsumptionModelsGenerator
                .Create(foods, random);
            var singleValueConcentrationModels = MockSingleValueConcentrationModelsGenerator
                .Create(baseFoods, substances, random);
            var unitVariabilityFactors = MockUnitVariabilityFactorsGenerator
                .Create(baseFoods, substances, random, 0.1);

            var iestiSpecialCases = new List<IestiSpecialCase>() {new IestiSpecialCase(){
                Food = foods.First(),
                Substance = substances.First(),
                ApplicationTypeString = HarvestApplicationType.PostHarvest.ToString(),
                }
            };

            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstances = substances,
                ReferenceCompound = substances.First(),
                SingleValueConsumptionModels = singleValueConsumptionModels,
                ActiveSubstanceSingleValueConcentrations = singleValueConcentrationModels,
                UnitVariabilityDictionary = unitVariabilityFactors,
                ProcessingFactorModels = processingFactors,
                SingleValueConsumptionIntakeUnit = ConsumptionIntakeUnit.gPerDay,
                SingleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg,
                IestiSpecialCases = iestiSpecialCases,
            };
            var project = new ProjectDto();
            project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod = method;
            var calculator = new SingleValueDietaryExposuresActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestCompute-{method.GetShortDisplayName()}");
        }

        private void testChronicCalculationMethodProcessing(
            SingleValueDietaryExposuresCalculationMethod method,
            IRandom random
        ) {
            var baseFoods = MockFoodsGenerator.CreateFoodsWithUnitWeights(5, random, fractionMissing: .1);
            var processingTypes = MockProcessingTypesGenerator.Create(3);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(baseFoods, processingTypes);
            var foods = baseFoods.Concat(processedFoods).ToList();

            var substances = MockSubstancesGenerator.Create(3);
            var processingFactors = MockProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(foods, substances, processingTypes, random, fractionMissing: .1);
            var singleValueConsumptionModels = MockSingleValueConsumptionModelsGenerator
                .Create(foods, random);
            var singleValueConcentrationModels = MockSingleValueConcentrationModelsGenerator
                .Create(foods, substances, random);
            var data = new ActionData() {
                ModelledFoods = foods,
                ActiveSubstances = substances,
                ReferenceCompound = substances.First(),
                SingleValueConsumptionModels = singleValueConsumptionModels,
                ActiveSubstanceSingleValueConcentrations = singleValueConcentrationModels,
                ProcessingFactorModels = processingFactors,
                SingleValueConsumptionIntakeUnit = ConsumptionIntakeUnit.gPerDay,
                SingleValueConsumptionBodyWeightUnit = BodyWeightUnit.kg,
            };
            var project = new ProjectDto();
            project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod = method;
            var calculator = new SingleValueDietaryExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, $"TestCompute{method.GetShortDisplayName()}-Processing");
        }
    }
}
