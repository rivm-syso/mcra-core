using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            section.Records = [];
            section.HbmPercentilesRecords = [];
            section.HbmPercentilesAllRecords = [];
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test HbmSamplesBySamplingMethodSubstanceSection view
        /// </summary>
        [TestMethod]
        public void HbmSamplesBySamplingMethodSubstanceSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var section = new HbmSamplesBySamplingMethodSubstanceSection();
            section.Summarize(
                [],
                hbmSampleSubstanceCollections,
                substances,
                25,
                75,
                [],
                true
            );
            Assert.AreEqual(substances.Count, section.HbmPercentilesRecords[samplingMethod].Count);
            Assert.AreEqual(substances.Count, section.HbmPercentilesAllRecords[samplingMethod].Count);
        }
    }
}