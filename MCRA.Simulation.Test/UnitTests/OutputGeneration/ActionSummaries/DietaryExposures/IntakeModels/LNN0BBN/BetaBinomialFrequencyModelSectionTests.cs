using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                FrequencyModelEstimates = [new ParameterEstimates()],
                LikelihoodRatioTestResults = new LikelihoodRatioTestResults() {
                    DfPolynomial = [4],
                    LogLikelihood = [11111],
                    DegreesOfFreedom = [4],
                    DeltaChi = [0.05],
                    DeltaDf = [4],
                    PValue = [0.05],
                }
            };
            AssertIsValidView(section);
        }
    }
}


