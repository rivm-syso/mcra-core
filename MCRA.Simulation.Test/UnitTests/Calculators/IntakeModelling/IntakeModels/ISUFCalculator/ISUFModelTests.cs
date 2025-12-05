using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {

    /// <summary>
    /// ISUF model calculator tests.
    /// </summary>
    [TestClass]
    public class ISUFModelTests {

        /// <summary>
        /// Calculate parameters ISUF model: TransformType.Logarithmic, IsSpline = false
        /// </summary>
        [TestMethod]
        public void ISUFModel_TestCalculateParametersLogarithmic() {
            //for loop met verschillende seeds
            //1: test exactheid
            //2: test robuustheid stochastiek, tolerance, probably a class of test outside MCRA
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(200, 2, true, random);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.1, random);
            var model = new ISUFModel(TransformType.Logarithmic, new ISUFModelCalculationSettings(
                gridPrecision: 20,
                numberOfIterations: 5,
                isSplineFit: false
            ));
            model.CalculateParameters(individualDayIntakes);
            Assert.IsGreaterThan(0.75, model.TransformationResult.VarianceBetweenUnit);
            Assert.IsLessThanOrEqualTo(0.25, model.TransformationResult.VarianceWithinUnit);
            Assert.HasCount(21, model.FrequencyResult.DiscreteFrequencies);
        }

        /// <summary>
        /// Calculate parameters ISUF model: TransformType.Logarithmic, IsSpline = true
        /// </summary>
        [TestMethod]
        public void ISUFModel_TestCalculateParametersLogarithmicSpline() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(200, 2, true, random);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.2, random);
            var model = new ISUFModel(TransformType.Logarithmic, new ISUFModelCalculationSettings(
                gridPrecision: 20,
                numberOfIterations: 5,
                isSplineFit: true
            ));
            model.CalculateParameters(individualDayIntakes);
            Assert.IsGreaterThan(0.75, model.TransformationResult.VarianceBetweenUnit);
            Assert.IsLessThanOrEqualTo(0.25, model.TransformationResult.VarianceWithinUnit);
        }

        /// <summary>
        /// Calculate parameters ISUF model: TransformType.NoTransform, IsSpline = false
        /// </summary>
        [TestMethod]
        public void ISUFModel_TestCalculateParametersNoTransform() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(200, 2, true, random);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var model = new ISUFModel(TransformType.NoTransform, new ISUFModelCalculationSettings(
                gridPrecision : 20,
                numberOfIterations : 5,
                isSplineFit : false
            ));
            model.CalculateParameters(individualDayIntakes);
            Assert.IsGreaterThan(0.75, model.TransformationResult.VarianceBetweenUnit);
            Assert.IsLessThanOrEqualTo(0.25, model.TransformationResult.VarianceWithinUnit);
        }

        /// <summary>
        /// Calculate parameters ISUF model: TransformType.Power, IsSpline = false
        /// </summary>
        [TestMethod]
        public void ISUFModel_TestCalculateParametersPower() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(200, 2, true, random);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var model = new ISUFModel(TransformType.Power, new ISUFModelCalculationSettings(
                gridPrecision: 20,
                numberOfIterations: 5,
                isSplineFit: false
            ));
            model.CalculateParameters(individualDayIntakes);
            Assert.IsGreaterThan(0.75, model.TransformationResult.VarianceBetweenUnit);
            Assert.IsLessThanOrEqualTo(0.25, model.TransformationResult.VarianceWithinUnit);
        }
        /// <summary>
        /// Calculate parameters ISUF model: TransformType.Power, IsSpline = true
        /// </summary>
        [TestMethod]
        public void ISUFModel_TestCalculateParametersPowerSpline() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(200, 2, true, random);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var model = new ISUFModel(TransformType.Power, new ISUFModelCalculationSettings(
                gridPrecision: 20,
                numberOfIterations: 5,
                isSplineFit: true
            ));
            model.CalculateParameters(individualDayIntakes);
            Assert.IsGreaterThan(0.75, model.TransformationResult.VarianceBetweenUnit);
            Assert.IsLessThanOrEqualTo(0.25, model.TransformationResult.VarianceWithinUnit);
        }
    }
}