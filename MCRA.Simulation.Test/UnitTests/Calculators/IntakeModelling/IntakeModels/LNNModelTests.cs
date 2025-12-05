using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {

    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class LNNModelTests {

        /// <summary>
        /// Creates model factory and calculates parameters LNN model, CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void LNNModel_TestCalculateParametersConstant() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(200, 2, true, random, null);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.5, random);
            var model = new LNNModel(
                new IntakeModelCalculationSettings(
                    covariateModelType: CovariateModelType.Cofactor
                ),
                new IntakeModelCalculationSettings(
                    covariateModelType: CovariateModelType.Cofactor
                )
            );
            model.CalculateParameters(individualDayIntakes);

            var summaryAmounts = model.FrequencyAmountModelSummary;
            Assert.IsGreaterThan(0.3, summaryAmounts.VarianceBetween);
            Assert.IsGreaterThan(0.05, summaryAmounts.VarianceWithin);
            Assert.IsFalse(double.IsNaN(summaryAmounts._2LogLikelihood));
        }

        /// <summary>
        /// Creates model factory and calculates parameters LNN model, CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void LNNModel_TestCalculateParametersCofactor() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, true, random, properties);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.5, random);
            var model = new LNNModel(
                new IntakeModelCalculationSettings(
                    covariateModelType: CovariateModelType.Cofactor
                ),
                new IntakeModelCalculationSettings(
                    covariateModelType: CovariateModelType.Covariable,
                    minDegreesOfFreedom: 2,
                    maxDegreesOfFreedom: 2
                ),
                [2, 4, 6, 10]
            ) {
                TransformType = TransformType.Logarithmic
            };
            model.CalculateParameters(individualDayIntakes);

            var lnnModel = model;
            var summaryAmounts = lnnModel.FrequencyAmountModelSummary;
            Assert.IsGreaterThan(0.3, summaryAmounts.VarianceBetween);
            Assert.IsGreaterThan(0.05, summaryAmounts.VarianceWithin);
            Assert.IsFalse(double.IsNaN(summaryAmounts._2LogLikelihood));
        }
    }
}