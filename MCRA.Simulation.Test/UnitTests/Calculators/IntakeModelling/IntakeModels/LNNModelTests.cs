using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                new FrequencyModelCalculationSettings(new() { FrequencyModelCovariateModelType = CovariateModelType.Cofactor }),
                new AmountModelCalculationSettings(new() { AmountModelCovariateModelType = CovariateModelType.Cofactor })
            );
            model.CalculateParameters(individualDayIntakes);

            var summaryAmounts = model.FrequencyAmountModelSummary;
            Assert.IsTrue(summaryAmounts.VarianceBetween > 0.3);
            Assert.IsTrue(summaryAmounts.VarianceWithin > 0.05);
            Assert.IsTrue(!double.IsNaN(summaryAmounts._2LogLikelihood));
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
                new FrequencyModelCalculationSettings(new() { FrequencyModelCovariateModelType = CovariateModelType.Cofactor }),
                new AmountModelCalculationSettings(new() {
                    AmountModelCovariateModelType = CovariateModelType.Covariable,
                    AmountModelMinDegreesOfFreedom = 2,
                    AmountModelMaxDegreesOfFreedom = 2
                }),
                 [2, 4, 6, 10]
            ) {
                TransformType = TransformType.Logarithmic
            };
            model.CalculateParameters(individualDayIntakes);

            var lnnModel = model;
            var summaryAmounts = lnnModel.FrequencyAmountModelSummary;
            Assert.IsTrue(summaryAmounts.VarianceBetween > 0.3);
            Assert.IsTrue(summaryAmounts.VarianceWithin > 0.05);
            Assert.IsTrue(!double.IsNaN(summaryAmounts._2LogLikelihood));
        }
    }
}