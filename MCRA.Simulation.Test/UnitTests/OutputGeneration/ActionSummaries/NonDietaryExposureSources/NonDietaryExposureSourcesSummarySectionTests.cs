using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Foods {

    /// <summary>
    /// OutputGeneration, ActionSummaries, NonDietaryExposureSources.
    /// </summary>
    [TestClass]
    public class NonDietaryExposureSourcesSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test whether the summary section has a valid view.
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureSourcesSummarySection_TestHasValidView() {
            var section = new NonDietaryExposureSourcesSummarySection {
                Records = []
            };
            section.Records.Add(new NonDietaryExposureSourceSummaryRecord() { Code = "A", Name = "Aftershave" });
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test summarize.
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureSourcesSummarySection_TestSummarize() {
            var fakes = FakeNonDietaryExposureSourcesGenerator.Create(5);
            var section = new NonDietaryExposureSourcesSummarySection();
            section.Summarize(fakes);
            Assert.AreEqual(5, section.Records.Count);
            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarize.html");
        }
    }
}