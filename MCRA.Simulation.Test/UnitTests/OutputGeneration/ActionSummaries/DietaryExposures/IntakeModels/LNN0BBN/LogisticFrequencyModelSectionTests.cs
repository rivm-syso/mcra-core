using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, LNN0BBN
    /// </summary>
    [TestClass]
    public class LogisticFrequencyModelSectionTests : SectionTestBase {
        /// <summary>
        /// Test LogisticFrequencyModelSection view
        /// </summary>
        [TestMethod]
        public void LogisticFrequencyModelSection_Test1() {
            var section = new LogisticFrequencyModelSection() {
                DispersionEstimates = new ParameterEstimates(),
                VarianceEstimates = new ParameterEstimates(),
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


