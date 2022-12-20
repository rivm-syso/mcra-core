using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {

    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class MonteCarloIntegratorTests {

        /// <summary>
        /// Calculate parameters BBN model and performs Monte Carlo integration, CovariateModelType.Covariable
        /// </summary>
        [TestMethod]
        public void MonteCarloIntegratorTest1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayAmounts = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.5, random);
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(individualDayAmounts);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayAmounts);

            var covariableValues = individualDays
                .OrderBy(r => r.Individual.Covariable)
                .Select(r => r.Individual.Covariable)
                .Distinct()
                .ToList();

            var predictionLevels = new List<double> { covariableValues.First(), covariableValues.Last() };
            var frequencyModel = new BetaBinomialFrequencyModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05
            };
            frequencyModel.CalculateParameters(individualIntakeFrequencies, predictionLevels);

            var amountsModel = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 4,
                CovariateModel = CovariateModelType.Covariable,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                TransformType = TransformType.NoTransform,
            };
            amountsModel.CalculateParameters(individualAmounts, predictionLevels);

            var result = new MonteCarloIntegrator<FrequencyModel, AmountsModelBase> {
                FrequencyModel = frequencyModel,
                AmountsModel = amountsModel
            };
            var covariateGroups = predictionLevels
                .Select(r => new CovariateGroup() {
                    Covariable = r,
                    GroupSamplingWeight = 10,
                    NumberOfIndividuals = 5
                })
                .ToList();

            var marginals = result.CalculateMarginalIntakes(covariateGroups, seed);
            var conditionals = result.CalculateConditionalIntakes(covariateGroups, seed);
            var individuals = result.CalculateIndividualIntakes(seed);
            var meanMarginals = marginals.First().ModelBasedIntakes.Average();
            var meanConditionals = conditionals.First().ConditionalUsualIntakes.Average();
            var meanIndividuals = individuals.Average(c => c.UsualIntake);
            Assert.IsTrue(meanMarginals > 1);
            Assert.IsTrue(meanConditionals > 1);
            Assert.IsTrue(meanIndividuals > 1);
        }

        /// <summary>
        /// Calculate parameters BBN model and performs Monte Carlo integration, CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void MonteCarloIntegratorTest2() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var individualDayAmounts = MockSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.5, random);
            var individualIntakeFrequencies = IndividualFrequencyCalculator.Compute(individualDayAmounts);
            var individualAmounts = MockSimpleIndividualIntakeGenerator.Create(individualDayAmounts);

            var predictionLevels = new List<double> { 2, 4, 6 };

            var frequencyModel = new BetaBinomialFrequencyModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 2,
                CovariateModel = CovariateModelType.Cofactor,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05
            };
            frequencyModel.CalculateParameters(individualIntakeFrequencies, predictionLevels);

            var amountsModel = new NormalAmountsModel() {
                MinDegreesOfFreedom = 0,
                MaxDegreesOfFreedom = 4,
                CovariateModel = CovariateModelType.Cofactor,
                Function = FunctionType.Polynomial,
                TestingMethod = TestingMethodType.Backward,
                TestingLevel = 0.05,
                TransformType = TransformType.NoTransform,
            };
            amountsModel.CalculateParameters(individualAmounts, predictionLevels);

            var result = new MonteCarloIntegrator<FrequencyModel, AmountsModelBase> {
                FrequencyModel = frequencyModel,
                AmountsModel = amountsModel
            };
            var covariateGroups = new List<CovariateGroup> { new CovariateGroup() { Cofactor = "male", GroupSamplingWeight = 10, NumberOfIndividuals = 5 } };
            var predictionCovariateGroups = new List<CovariateGroup> { new CovariateGroup() { Cofactor = "male", GroupSamplingWeight = 10, NumberOfIndividuals = 5 } };

            var marginals = result.CalculateMarginalIntakes(covariateGroups, seed);
            var conditionals = result.CalculateConditionalIntakes(predictionCovariateGroups, seed);
            var individuals = result.CalculateIndividualIntakes(seed);
            var meanMarginals = marginals.First().ModelBasedIntakes.Average();
            var meanConditionals = conditionals.First().ConditionalUsualIntakes.Average();
            var meanIndividuals = individuals.Average(c => c.UsualIntake);
            Assert.IsTrue(meanMarginals > 1);
            Assert.IsTrue(meanConditionals > 1);
            Assert.IsTrue(meanIndividuals > 1);
        }
    }
}
