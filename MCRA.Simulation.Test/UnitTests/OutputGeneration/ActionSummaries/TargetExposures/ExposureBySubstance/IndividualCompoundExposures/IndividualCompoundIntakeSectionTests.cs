using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureBySubstance, IndividualCompoundExposures
    /// </summary>
    [TestClass]
    public class IndividualCompoundIntakeSectionTests : SectionTestBase {
        /// <summary>
        /// Test IndividualCompoundIntakeSection view
        /// </summary>
        [TestMethod]
        public void IndividualCompoundIntakeSection_Test1() {
            var section = new IndividualSubstanceExposureSection() {
                Records = [
                    new IndividualSubstanceExposureRecord() {
                        Bodyweight = 75,
                        CumulativeExposure = 1.234,
                        Exposure = 2.468,
                        IndividualId = "12345",
                        NumberOfDaysInSurvey = 2,
                        SamplingWeight = 1,
                        SubstanceCode = "C"
                    }
                ],
            };
            AssertIsValidView(section);
        }
    }
}