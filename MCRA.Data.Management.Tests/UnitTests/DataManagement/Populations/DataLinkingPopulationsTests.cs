using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingPopulationsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingPopulationsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Populations, @"PopulationsTests\Populations"),
                (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests\PopulationIndividualPropertyValues")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Populations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Populations);
            AssertDataReadingSummaryRecord(report, ScopingType.Populations, 10, "DE,DE-N,DE-W,DE-S,DE-E,DE-Summer,DE-Winter,DE-PopulationA,DE-PopulationB,DE-PopulationC", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.PopulationIndividualPropertyValues, ScopingType.Populations, 8, "DE-N,DE-W,DE-S,DE-E,DE-PopulationB,DE-PopulationC", "XXX,DE-PopulationAWoman", "DE,DE-Summer,DE-Winter,DE-PopulationA");
        }

        [TestMethod]
        public void DataLinkingPopulationsFilterTest() {
            _rawDataProvider.SetDataTables(
                 (ScopingType.Populations, @"PopulationsTests\Populations"),
                 (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests\PopulationIndividualPropertyValues")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Populations, ["DE-N"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.Populations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Populations);
            AssertDataLinkingSummaryRecord(report, ScopingType.PopulationIndividualPropertyValues, ScopingType.Populations, 8, "DE-N", "DE-W,DE-S,DE-E,XXX,DE-PopulationAWoman,DE-PopulationB,DE-PopulationC", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Populations);
            AssertDataReadingSummaryRecord(report, ScopingType.Populations, 10, "DE-N", "DE,DE-W,DE-S,DE-E,DE-Summer,DE-Winter,DE-PopulationA,DE-PopulationB,DE-PopulationC", "");
        }
    }
}
