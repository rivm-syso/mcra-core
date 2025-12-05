using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.PredictionLevelsCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {

    /// <summary>
    /// Covariate groups calculator tests.
    /// </summary>
    [TestClass]
    public class CovariateGroupsCalculatorTests {

        /// <summary>
        /// Test Covariate Group: CovariateModelType.Constant
        /// </summary>
        [TestMethod]
        public void CovariateGroupCalculator_TestConstant() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);
            var calculator = new CovariateGroupCalculator(predictionLevels, CovariateModelType.Constant, CovariateModelType.Constant);

            var dataBasedCovariateGroups = calculator.ComputeDataBasedCovariateGroups(exposures);
            Assert.HasCount(1, dataBasedCovariateGroups);

            var specifiedPredictionsCovariateGroups = calculator.ComputeSpecifiedPredictionsCovariateGroups(exposures);
            Assert.HasCount(1, specifiedPredictionsCovariateGroups);
        }

        /// <summary>
        /// Test Covariate Group: CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void CovariateGroupCalculator_TestCofactor() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);

            var calculator = new CovariateGroupCalculator(
                predictionLevels,
                CovariateModelType.Cofactor,
                CovariateModelType.Cofactor
            );

            var dataBasedCovariateGroups = calculator.ComputeDataBasedCovariateGroups(exposures);
            Assert.HasCount(2, dataBasedCovariateGroups);

            var specifiedPredictionsCovariateGroups = calculator.ComputeSpecifiedPredictionsCovariateGroups(exposures);
            Assert.HasCount(2, specifiedPredictionsCovariateGroups);
        }

        /// <summary>
        /// Test Covariate Group: CovariateModelType.Covariable
        /// </summary>
        [TestMethod]
        public void CovariateGroupCalculator_TestCovariable() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);

            var calculator = new CovariateGroupCalculator(
                predictionLevels,
                CovariateModelType.Covariable,
                CovariateModelType.Covariable
            );

            var dataBasedCovariateGroups = calculator.ComputeDataBasedCovariateGroups(exposures);
            Assert.HasCount(20, dataBasedCovariateGroups);

            var specifiedPredictionsCovariateGroups = calculator.ComputeSpecifiedPredictionsCovariateGroups(exposures);
            Assert.HasCount(20, specifiedPredictionsCovariateGroups);
        }

        /// <summary>
        /// Test Covariate Group: CovariateModelType.CovariableCofactor
        /// </summary>
        [TestMethod]
        public void CovariateGroupCalculator_TestCovariableCofactor() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);

            var calculator = new CovariateGroupCalculator(
                predictionLevels,
                CovariateModelType.CovariableCofactor,
                CovariateModelType.CovariableCofactor
            );

            var dataBasedCovariateGroups = calculator.ComputeDataBasedCovariateGroups(exposures);
            Assert.HasCount(20, dataBasedCovariateGroups);

            var specifiedPredictionsCovariateGroups = calculator.ComputeSpecifiedPredictionsCovariateGroups(exposures);
            Assert.HasCount(40, specifiedPredictionsCovariateGroups);
        }

        /// <summary>
        /// Test Covariate Group: CovariateModelType.CovariableCofactorInteraction
        /// </summary>
        [TestMethod]
        public void CovariateGroupCalculator_TestCovariableCofactorInt() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 20, []);

            var calculator = new CovariateGroupCalculator(
                predictionLevels,
                CovariateModelType.CovariableCofactorInteraction,
                CovariateModelType.CovariableCofactorInteraction
            );

            var dataBasedCovariateGroups = calculator.ComputeDataBasedCovariateGroups(exposures);
            Assert.HasCount(20, dataBasedCovariateGroups);

            var specifiedPredictionsCovariateGroups = calculator.ComputeSpecifiedPredictionsCovariateGroups(exposures);
            Assert.HasCount(40, specifiedPredictionsCovariateGroups);
        }
    }
}
