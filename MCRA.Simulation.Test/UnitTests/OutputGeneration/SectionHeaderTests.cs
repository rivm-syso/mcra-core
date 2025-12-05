using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {
    /// <summary>
    /// OutputGeneration
    /// </summary>
    [TestClass]
    public class SectionHeaderTests {

        private SummaryToc _testRoot;

        /// <summary>
        /// Initialize test: create a section header with 3 levels deep subheaders 0-F hex
        /// </summary>
        [TestInitialize]
        public void TestInitialize() {
            _testRoot = new SummaryToc {
                Name = "Root",
                HasSectionData = false,
                SectionId = Guid.Empty,
                SectionTypeName = "TestRoot",
                SummarySectionName = "TestRoot",
            };
            for (var i = 1; i <= 15; i++) {
                var hdri = _testRoot.AddEmptySubSectionHeader($"Title{i:X1}", i, sectionId: new Guid($"{i:X1}0000000-0000-0000-0000-000000000000"), sectionLabel: $"sectionLabel{i:X1}");
                for (var j = 1; j <= 15; j++) {
                    var hdrj = hdri.AddEmptySubSectionHeader($"Title{j:X1}", j, sectionId: new Guid($"{i:X1}{j:X1}000000-0000-0000-0000-000000000000"), sectionLabel: $"sectionLabel{i:X1}{j:X1}");
                    for (var h = 1; h <= 15; h++) {
                        _ = hdrj.AddEmptySubSectionHeader($"Title{h:X1}", h, sectionId: new Guid($"{i:X1}{j:X1}{h:X1}00000-0000-0000-0000-000000000000"), sectionLabel: $"sectionLabel{i:X1}{j:X1}{h:X1}");
                    }
                }
            }
        }

        /// <summary>
        /// Test section headers
        /// </summary>
        [TestMethod]
        public void SectionHeader_Test() {
            var dietarySection = new DietaryIntakeDistributionSection();

            var header = new SectionHeader() {
                Name = "test",
                Depth = 1,
                HasSectionData = true,
                Order = 0,
                SectionId = Guid.NewGuid(),
                SectionTypeName = "test",
                SummarySectionName = "test",
            };
            Assert.AreEqual("test", header.Name);

            var header1 = new SectionHeader() { Name = "test" };
            Assert.AreEqual("test", header1.Name);

            var subHeader = header.AddSubSectionHeaderFor(dietarySection, "DietaryIntakeDistributionSection", 0);
            Assert.AreEqual("MCRA.Simulation.OutputGeneration.DietaryIntakeDistributionSection", subHeader.SectionTypeName);
            var percentileSection = new IntakePercentileSection() {  };
            var subSubHeader = subHeader.AddSubSectionHeaderFor(percentileSection, "IntakePercentileSection", 0);

            var sectionPercentile = subSubHeader.GetSummarySection() as IntakePercentileSection;
            //Assert.AreEqual(sectionPercentile.Reference.Name, "referenceName");

            subHeader = header.GetSubSectionHeader<DietaryIntakeDistributionSection>();
            Assert.AreEqual("DietaryIntakeDistributionSection", subHeader.SummarySectionName);

            _ = subHeader.AddSubSectionHeaderFor(new IntakePercentileSection(), "dietary", 0);
            subSubHeader = subHeader.GetSubSectionHeaderFromTitleString<IntakePercentileSection>("DIETARY");
            Assert.IsNull(subSubHeader);

            subSubHeader = header.GetSubSectionHeaderFromTitleString<IntakePercentileSection>("dietary");
            Assert.AreEqual("dietary", subSubHeader.Name);

            var guid = Guid.NewGuid();
            subSubHeader = header.GetSubSectionHeader(guid);
            Assert.IsNull(subSubHeader);
        }

        /// <summary>
        /// Tests retrieval of section headers by specified path.
        /// </summary>
        [TestMethod]
        public void SectionHeaderGetSubsectionByPathTest() {
            Assert.IsNotNull(_testRoot);

            var hdr = _testRoot.GetSubSectionHeaderByTitlePath();
            Assert.IsNull(hdr);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath(null);
            Assert.IsNull(hdr);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath("noway");
            Assert.IsNull(hdr);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath("a", null, "", "Null");
            Assert.IsNull(hdr);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath("titleF");
            Assert.IsNotNull(hdr);
            Assert.AreEqual(new Guid("F0000000-0000-0000-0000-000000000000"), hdr.SectionId);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath("title1", "TITLEa", "TitleD");
            Assert.IsNotNull(hdr);
            Assert.AreEqual(new Guid("1AD00000-0000-0000-0000-000000000000"), hdr.SectionId);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath("title1", "TITLEa", "TitleD", "");
            Assert.IsNull(hdr);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath("title1", "TITLEa", "TitleD", "TitleE", "TitleF");
            Assert.IsNull(hdr);

            hdr = _testRoot.GetSubSectionHeaderByTitlePath("title1", "TITLEa");
            var guidTest = Guid.NewGuid();
            //add duplicate title sub section header "Title2" under "Title1/TitleA"
            //with higher order
            hdr.AddEmptySubSectionHeader("Title2", 1, sectionId: guidTest);
            var hdrTest = _testRoot.GetSubSectionHeaderByTitlePath("Title1", "titlea", "title2");
            //should return the new guid
            Assert.AreEqual(guidTest, hdrTest.SectionId);
            //add another same title under a different Guid but lower order
            //should not change the outcome
            hdr.AddEmptySubSectionHeader("Title2", 2, sectionId: Guid.NewGuid());
            hdrTest = _testRoot.GetSubSectionHeaderByTitlePath("Title1", "titlea", "title2");
            Assert.AreEqual(guidTest, hdrTest.SectionId);
        }

        /// <summary>
        /// Tests retrieval of section headers by specified section label.
        /// </summary>
        [TestMethod]
        public void SectionHeaderGetSubsectionBySectionLabelTest() {
            Assert.IsNotNull(_testRoot);
            var hdr = _testRoot.GetSubSectionHeaderBySectionLabel(null);
            Assert.IsNotNull(hdr);
            hdr = _testRoot.GetSubSectionHeaderBySectionLabel("test");
            Assert.IsNull(hdr);
            hdr = _testRoot.GetSubSectionHeaderBySectionLabel("sectionLabel1");
            Assert.IsNotNull(hdr);
            hdr = _testRoot.GetSubSectionHeaderBySectionLabel("sectionLabelF1");
            Assert.IsNotNull(hdr);
        }
    }
}
