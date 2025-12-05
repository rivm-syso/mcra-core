using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class BBNModelTests {

        /// <summary>
        /// Creates model factory and calculates parameters BBN model, CovariateModelType.Constant
        /// </summary>
        [TestMethod]
        public void BBNModel_TestCalculateParametersConstant() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.2, random);
            var bbnModel = new BBNModel(
                new IntakeModelCalculationSettings(),
                new IntakeModelCalculationSettings()
            );
            bbnModel.CalculateParameters(individualDayIntakes);

            var summaryAmounts = bbnModel.AmountsModelSummary as NormalAmountsModelSummary;
            var summaryFrequencies = bbnModel.FrequencyModelSummary;

            Assert.IsGreaterThan(0.2, summaryAmounts.VarianceBetween);
            Assert.IsGreaterThan(0.01, summaryAmounts.VarianceWithin);
            Assert.IsFalse(double.IsNaN(summaryAmounts._2LogLikelihood));
            Assert.IsFalse(double.IsNaN(summaryFrequencies._2LogLikelihood));
            Assert.IsGreaterThan(0, summaryFrequencies.DegreesOfFreedom);
        }

        /// <summary>
        /// Creates model factory and calculates parameters BBN model, CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void BBNModel_TestCalculateParametersCofactor() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.2, random);

            var bbnModel = new BBNModel(
                new IntakeModelCalculationSettings(
                    covariateModelType: CovariateModelType.Cofactor,
                    testingLevel : 0.05
                ),
                new IntakeModelCalculationSettings(
                    covariateModelType: CovariateModelType.Cofactor
                )
            ) {
                TransformType = TransformType.Logarithmic
            };
            bbnModel.CalculateParameters(individualDayIntakes);

            var summaryAmounts = bbnModel.AmountsModelSummary as NormalAmountsModelSummary;
            var summaryFrequencies = bbnModel.FrequencyModelSummary;

            Assert.IsGreaterThan(0.2, summaryAmounts.VarianceBetween);
            Assert.IsGreaterThan(0.01, summaryAmounts.VarianceWithin);
            Assert.IsFalse(double.IsNaN(summaryAmounts._2LogLikelihood));
            Assert.IsFalse(double.IsNaN(summaryFrequencies._2LogLikelihood));
            Assert.AreEqual(52, summaryFrequencies.DegreesOfFreedom);
        }
    }
}
