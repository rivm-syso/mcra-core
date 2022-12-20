using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringData {
    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringData, Samples
    /// </summary>
    [TestClass]
    public class HbmSamplesBySamplingMethodSubstanceSectionTests : SectionTestBase {

        /// <summary>
        /// Test HbmSamplesBySamplingMethodSubstanceSection view
        /// </summary>
        [TestMethod]
        public void HbmSamplesBySamplingMethodSubstanceSection_TestValidView() {
            var section = new HbmSamplesBySamplingMethodSubstanceSection();
            section.Records = new List<HbmSamplesBySamplingMethodSubstanceRecord>();
            section.HbmPercentilesRecords = new List<HbmSampleConcentrationPercentilesRecord>();
            section.HbmPercentilesAllRecords = new List<HbmSampleConcentrationPercentilesRecord>();
            AssertIsValidView(section);
        }
    }
}