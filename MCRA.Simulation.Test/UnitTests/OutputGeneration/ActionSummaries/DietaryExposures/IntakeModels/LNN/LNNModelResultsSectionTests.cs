using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, LNN
    /// </summary>
    [TestClass]
    public class LNNModelResultsSectionTests : SectionTestBase {
        /// <summary>
        /// Test LNNModelResultsSection view 
        /// </summary>
        [TestMethod]
        public void LNNModelResultsSection_Test1() {
            var section = new LNNModelResultsSection() {
                VarianceEstimates = new ParameterEstimates(),
                FrequencyModelEstimates = new List<ParameterEstimates>(),
                AmountsModelEstimates = new List<ParameterEstimates>(),
                CorrelationEstimates= new ParameterEstimates(),
            };
            AssertIsValidView(section);
        }
    }
}


