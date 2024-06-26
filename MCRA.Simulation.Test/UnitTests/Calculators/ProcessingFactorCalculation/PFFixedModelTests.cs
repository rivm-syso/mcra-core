﻿using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class PFFixedModelTests : PFModelTestsBase {

        /// <summary>
        /// Fixed processing factors
        /// </summary>
        [TestMethod]
        public void PFFixedModel_Test1() {
            var pf = mockProcessingFactor(ProcessingDistributionType.LogNormal, 0.6, 0.7, 0.8, 0.9);

            var model = new PFFixedModel();
            model.CalculateParameters(pf);

            (var nominal, var isCorrectNominal) = model.GetNominalValue();
            Assert.AreEqual(0.6, nominal);
            Assert.IsTrue(isCorrectNominal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            (var draw, var isCorrectDraw) = model.DrawFromDistribution(random);
            Assert.AreEqual(0.6, draw);
            Assert.IsTrue(isCorrectDraw);

            var n = 500;
            var samples = runUncertains(model, random, n);
            Assert.AreEqual(0.6, samples.Median(), 1e-1);
        }
    }
}
