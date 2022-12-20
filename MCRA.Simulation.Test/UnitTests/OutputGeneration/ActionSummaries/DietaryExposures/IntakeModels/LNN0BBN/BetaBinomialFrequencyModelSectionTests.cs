using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, LNN0BBN
    /// </summary>
    [TestClass]
    public class BetaBinomialFrequencyModelSectionTests : SectionTestBase {
        /// <summary>
        /// Test BetaBinomialFrequencyModelSection view 
        /// </summary>
        [TestMethod]
        public void BetaBinomialFrequencyModelSection_Test1() {
            var section = new BetaBinomialFrequencyModelSection() {
                DispersionEstimates = new ParameterEstimates(),
                FrequencyModelEstimates = new List<ParameterEstimates>() { new ParameterEstimates() },
                LikelihoodRatioTestResults = new LikelihoodRatioTestResults() {
                    DfPolynomial = new List<int> { 4 },
                    LogLikelihood = new List<double>() { 11111 },
                    DegreesOfFreedom = new List<int> { 4 },
                    DeltaChi = new List<double>() { 0.05 },
                    DeltaDf = new List<int> { 4 },
                    PValue = new List<double>() { 0.05 },
                }
            };
            AssertIsValidView(section);
        }
    }
}


