using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.PredictionLevelsCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {

    /// <summary>
    /// Logistic normal frequency model calculator tests.
    /// </summary>
    [TestClass]
    public class LogisticNormalFrequencyModelTests {

        /// <summary>
        /// Calculate parameters LogisticNormal Frequency model: test fit model with all 0%
        /// intake frequencies.
        /// </summary>
        [TestMethod]
        public void LogisticNormalFrequencyModel_TestAllFrequencyZero() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(5, 2, true, random);
            var individualIntakeFrequencies = FakeIndividualFrequenciesGenerator.Create(individualDays, 0, 0);
            var model = new LogisticNormalFrequencyModel();
            var result = model.CalculateParameters(individualIntakeFrequencies, null);
            Assert.AreEqual(ErrorMessages.ModelIsSkippedEqualFrequencies, result.ErrorMessage);
        }

        /// <summary>
        /// Calculate parameters LogisticNormal Frequency model: test fit model with all 50%
        /// intake frequencies.
        /// </summary>
        [TestMethod]
        public void LogisticNormalFrequencyModel_TestAll50PercentIntake() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(5, 2, true, random);
            var individualIntakeFrequencies = FakeIndividualFrequenciesGenerator.Create(individualDays, 0.5, 0.5);
            var model = new LogisticNormalFrequencyModel();
            var result = model.CalculateParameters(individualIntakeFrequencies, null);
            Assert.AreEqual(ErrorMessages.ModelIsSkippedEqualFrequencies, result.ErrorMessage);
        }

        /// <summary>
        /// Calculate parameters LogisticNormal Frequency model: test fit model with all 100%
        /// intake frequencies.
        /// </summary>
        [TestMethod]
        public void LogisticNormalFrequencyModel_TestAll100PercentIntake() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(5, 2, true, random);
            var individualIntakeFrequencies = FakeIndividualFrequenciesGenerator.Create(individualDays, 1, 1);
            var model = new LogisticNormalFrequencyModel();
            var result = model.CalculateParameters(individualIntakeFrequencies, null);
            Assert.AreEqual(ErrorMessages.ModelIsSkipped100Frequencies, result.ErrorMessage);
        }

        /// <summary>
        /// Calculate parameters LogisticNormal Frequency model: CovariateModelType.Constant
        /// </summary>
        [TestMethod]
        public void LogisticNormalFrequencyModell_TestFitConstant() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, null);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(exposures);
            var model = new LogisticNormalFrequencyModel() {
                CovariateModel = CovariateModelType.Constant,
            };
            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);
        }

        /// <summary>
        /// Calculate parameters LogisticNormal Frequency model: CovariateModelType.Covariable
        /// </summary>
        [TestMethod]
        public void LogisticNormalFrequencyModel_TestFitCovariable() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var predictionLevels = new List<double> { 0, 2, 4 };
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(exposures);
            var model = new LogisticNormalFrequencyModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05
            };
            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);

            (_, _) = model.GetDistribution(model.ConditionalPredictions, new CovariateGroup() { Covariable = individualDays.First().SimulatedIndividual.Covariable, GroupSamplingWeight = 10, NumberOfIndividuals = 8 });
            model.TestingMethod = TestingMethodType.Forward;
            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);
            model.GetDefaultModelSummary(new ErrorMessages());
        }

        /// <summary>
        /// Calculate parameters LogisticNormal Frequency model: CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void LogisticNormalFrequencyModel_TestFitCofactor() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(exposures);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);
            var model = new LogisticNormalFrequencyModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Cofactor,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
            };

            model.CalculateParameters(individualIntakeFrequencies, predictionLevels);
            (_, _) = model.GetDistribution(
                model.ConditionalPredictions,
                new CovariateGroup() {
                    Cofactor = "male",
                    GroupSamplingWeight = 10,
                    NumberOfIndividuals = 8
                }
            );
        }
    }
}