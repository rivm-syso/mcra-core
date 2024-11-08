using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class NormalAmountsModelTests {

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Constant
        /// </summary>
        [TestMethod]
        public void NormalAmountsModel_CalculateParameters1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 0,
                CovariateModel = CovariateModelType.Constant,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
            };
            model.CalculateParameters(individualAmounts, []);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void NormalAmountsModel_CalculateParameters2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 4,
                CovariateModel = CovariateModelType.Cofactor,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                TransformType = TransformType.NoTransform
            };

            model.CalculateParameters(individualAmounts, []);
            model.TestingMethod = TestingMethodType.Forward;
            model.CalculateParameters(individualAmounts, []);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Covariable, TransformType.NoTransform
        /// </summary>
        [TestMethod]
        public void NormalAmountsModelNoTransform_CalculateParameters2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 4,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                TransformType = TransformType.NoTransform,
            };

            var predictionLevels = new List<double> { 0, 2, 4 };
            model.CalculateParameters(individualAmounts, predictionLevels);

            var predictions = model.SpecifiedPredictions;
            var chronicPredictions = model.GetDistribution(predictions, new CovariateGroup() { Covariable = 4, GroupSamplingWeight = 10, NumberOfIndividuals = 8 }, out CovariateGroup aa);
            model.TestingMethod = TestingMethodType.Forward;
            model.CalculateParameters(individualAmounts, predictionLevels);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Covariable, TransformType.Logarithmic
        /// </summary>
        [TestMethod]
        public void NormalAmountsModelLogarithmic_CalculateParameters2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 4,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                TransformType = TransformType.Logarithmic,
            };

            var predictionLevels = new List<double> { 0, 2, 4 };
            model.CalculateParameters(individualAmounts, predictionLevels);

            var predictions = model.SpecifiedPredictions;
            var chronicPredictions = model.GetDistribution(predictions, new CovariateGroup() { Covariable = 4, GroupSamplingWeight = 10, NumberOfIndividuals = 8 }, out CovariateGroup aa);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Covariable, TransformType.Power
        /// </summary>
        [TestMethod]
        public void NormalAmountsModelPower_CalculateParameters2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 4,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                TransformType = TransformType.Power
            };

            var predictionLevels = new List<double> { 0, 2, 4 };
            model.CalculateParameters(individualAmounts, predictionLevels);
            var predictions = model.SpecifiedPredictions;
            var chronicPredictions = model.GetDistribution(predictions, new CovariateGroup() { Covariable = 4, GroupSamplingWeight = 10, NumberOfIndividuals = 8 }, out CovariateGroup aa);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Covariable, TransformType.NoTransform
        /// </summary>
        [TestMethod]
        public void NormalAmountsModel_CalculateParameters3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
            };

            var predictionLevels = new List<double> { 0, 2, 4 };
            model.CalculateParameters(individualAmounts, predictionLevels);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Covariable, TransformType.Power. Only one day available.
        /// </summary>
        [TestMethod]
        public void NormalAmountsModel_CalculateParameters4() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                TransformType = TransformType.Power,
            };

            var predictionLevels = new List<double> { 0, 2, 4 };
            model.CalculateParameters(individualAmounts, predictionLevels);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Constant
        /// </summary>
        [TestMethod]
        public void NormalAmountsModel_CalculateParametersOneDay() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 1, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 0,
                CovariateModel = CovariateModelType.Constant,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
            };
            model.CalculateParameters(individualAmounts, []);
        }

        /// <summary>
        /// Calculate parameters Normal Amounts Model: CovariateModelType.Constant
        /// </summary>
        [TestMethod]
        public void NormalAmountsModel_CalculateParametersAcuteCovariateModelling() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20,2, true, random, properties);
            var individualDayIntakes = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0, random);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayIntakes);
            var model = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 0,
                CovariateModel = CovariateModelType.Constant,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                IsAcuteCovariateModelling = true
            };
            model.CalculateParameters(individualAmounts, []);
        }
    }
}

