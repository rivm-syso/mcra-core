﻿using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                new FrequencyModelCalculationSettings(new()),
                new AmountModelCalculationSettings(new())
            );
            bbnModel.CalculateParameters(individualDayIntakes);

            var summaryAmounts = bbnModel.AmountsModelSummary as NormalAmountsModelSummary;
            var summaryFrequencies = bbnModel.FrequencyModelSummary;

            Assert.IsTrue(summaryAmounts.VarianceBetween > 0.2);
            Assert.IsTrue(summaryAmounts.VarianceWithin > 0.01);
            Assert.IsTrue(!double.IsNaN(summaryAmounts._2LogLikelihood));
            Assert.IsTrue(!double.IsNaN(summaryFrequencies._2LogLikelihood));
            Assert.IsTrue(summaryFrequencies.DegreesOfFreedom > 0);
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
                new FrequencyModelCalculationSettings(new() {
                    FrequencyModelCovariateModelType = CovariateModelType.Cofactor,
                    FrequencyModelTestingLevel = 0.05
                }),
                new AmountModelCalculationSettings(new() { AmountModelCovariateModelType = CovariateModelType.Cofactor })
            ) {
                TransformType = TransformType.Logarithmic
            };
            bbnModel.CalculateParameters(individualDayIntakes);

            var summaryAmounts = bbnModel.AmountsModelSummary as NormalAmountsModelSummary;
            var summaryFrequencies = bbnModel.FrequencyModelSummary;

            Assert.IsTrue(summaryAmounts.VarianceBetween > 0.2);
            Assert.IsTrue(summaryAmounts.VarianceWithin > 0.01);
            Assert.IsTrue(!double.IsNaN(summaryAmounts._2LogLikelihood));
            Assert.IsTrue(!double.IsNaN(summaryFrequencies._2LogLikelihood));
            Assert.AreEqual(52, summaryFrequencies.DegreesOfFreedom);
        }
    }
}
