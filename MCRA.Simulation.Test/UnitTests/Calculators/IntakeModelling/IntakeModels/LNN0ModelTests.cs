﻿using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class LNN0ModelTests {

        /// <summary>
        /// Creates model factory and calculates parameters LNN0 model, CovariateModelType.Cofactor
        /// </summary>
        [TestMethod]
        public void LNN0Model_TestCalculateParametersCofactor() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(200, 2, true, random, null);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.5, random);
            var model = new LNN0Model(
                new IntakeModelCalculationSettings(covariateModelType: CovariateModelType.Cofactor),
                new IntakeModelCalculationSettings(covariateModelType: CovariateModelType.Cofactor)
            ) {
                TransformType = TransformType.Logarithmic
            };
            model.CalculateParameters(individualDayIntakes);
            var summaryAmounts = model.AmountsModelSummary as NormalAmountsModelSummary;
            var summaryFrequencies = model.FrequencyModelSummary;
            Assert.IsTrue(summaryAmounts.VarianceBetween > 0.3);
            Assert.IsTrue(summaryAmounts.VarianceWithin > 0.05);
            Assert.IsTrue(!double.IsNaN(summaryFrequencies._2LogLikelihood));
            Assert.IsTrue(!double.IsNaN(summaryAmounts._2LogLikelihood));
        }
    }
}
