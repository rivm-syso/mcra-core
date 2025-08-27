using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.PredictionLevelsCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {

    /// <summary>
    /// Beta-binomial frequency model calculator tests.
    /// </summary>
    [TestClass]
    public class BetaBinomialFrequencyModelTests {

        /// <summary>
        /// Calculate parameters beta-binomial frequency model: test fit model with all 0%
        /// intake frequencies.
        /// </summary>
        [TestMethod]
        public void BetaBinomialFrequencyModel_TestAllFrequencyZero() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(5, 2, true, random);
            var individualIntakeFrequencies = FakeIndividualFrequenciesGenerator.Create(individualDays, 0, 0);
            var model = new BetaBinomialFrequencyModel();
            var result = model.CalculateParameters(individualIntakeFrequencies, null);
            Assert.AreEqual(ErrorMessages.ModelIsSkippedEqualFrequencies, result.ErrorMessage);
        }

        /// <summary>
        /// Calculate parameters beta-binomial frequency model: test fit model with all 50%
        /// intake frequencies.
        /// </summary>
        [TestMethod]
        public void BetaBinomialFrequencyModel_TestAll50PercentIntake() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(5, 2, true, random);
            var individualIntakeFrequencies = FakeIndividualFrequenciesGenerator.Create(individualDays, 0.5, 0.5);
            var model = new BetaBinomialFrequencyModel();
            var result = model.CalculateParameters(individualIntakeFrequencies, null);
            Assert.AreEqual(ErrorMessages.ModelIsSkippedEqualFrequencies, result.ErrorMessage);
        }

        /// <summary>
        /// Calculate parameters beta-binomial frequency model: test fit model with all 100%
        /// intake frequencies.
        /// </summary>
        [TestMethod]
        public void BetaBinomialFrequencyModel_TestAll100PercentIntake() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(5, 2, true, random);
            var individualIntakeFrequencies = FakeIndividualFrequenciesGenerator.Create(individualDays, 1, 1);
            var model = new BetaBinomialFrequencyModel();
            var result = model.CalculateParameters(individualIntakeFrequencies, null);
            Assert.AreEqual(ErrorMessages.ModelIsSkipped100Frequencies, result.ErrorMessage);
        }

        /// <summary>
        /// Calculate parameters BetaBinomial Frequency model: CovariateModelType.Constant
        /// </summary>
        [TestMethod]
        public void BetaBinomialFrequencyModel_CalculateParameters1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(exposures);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);
            var model = new BetaBinomialFrequencyModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Constant,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
            };
            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);
        }

        /// <summary>
        /// Calculate parameters BetaBinomial Frequency model: CovariateModelType.Covariable
        /// </summary>
        [TestMethod]
        public void BetaBinomialFrequencyModel_CalculateParameters2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(exposures);
            var predictionLevels = new List<double> { 0, 2, 4 };
            var model = new BetaBinomialFrequencyModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05
            };
            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);
            Assert.AreEqual(20, model.GetConditionalPredictions().ConditionalPrediction.Count);

            (var chronicPredictions, _) = model.GetDistribution(
                model.ConditionalPredictions,
                new CovariateGroup() {
                    Covariable = individualDays.First().SimulatedIndividual.Covariable,
                    GroupSamplingWeight = 10,
                    NumberOfIndividuals = 8
                }
            );
            Assert.IsNotNull(chronicPredictions);

            model.TestingMethod = TestingMethodType.Forward;
            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);
            model.GetDefaultModelSummary(new ErrorMessages());
        }

        /// <summary>
        /// Calculate parameters BetaBinomial Frequency model: CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void BetaBinomialFrequencyModel_CalculateParameters3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(exposures);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);
            var model = new BetaBinomialFrequencyModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Cofactor,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
            };
            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);
            Assert.AreEqual(2, model.GetConditionalPredictions().ConditionalPrediction.Count);
        }
    }
}