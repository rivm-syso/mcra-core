using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, IndividualCompoundExposures
    /// </summary>
    [TestClass]
    public class IndividualCompoundIntakeSectionTests : SectionTestBase {
        /// <summary>
        /// Test IndividualCompoundIntakeSection view
        /// </summary>
        [TestMethod]
        public void IndividualCompoundIntakeSection_Test1() {
            var section = new IndividualCompoundIntakeSection() {
                IndividualCompoundIntakeRecords = new List<IndividualCompoundIntakeRecord>() {
                    new IndividualCompoundIntakeRecord() {
                        Bodyweight = 75,
                        CumulativeExposure = 1.234,
                        ExposureAmount = 2.468,
                        IndividualId = "12345",
                        NumberOfDaysInSurvey = 2,
                        RelativeCompartmentWeight = 0.023,
                        SamplingWeight = 1,
                        SubstanceCode = "C"
                    }
                },
            };
            AssertIsValidView(section);
        }
    }
}