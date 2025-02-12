using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingSubstanceConversionsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingSubstanceConversionsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceConversions, @"ResidueDefinitionsTests/ResidueDefinitionsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.ResidueDefinitions);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.Compounds);
            CollectionAssert.AreEqual(new[] { "A", "B", "C" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.ResidueDefinitions);
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceConversions, ScopingType.Compounds, 3, "A,B,C", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType. Compounds, 0, "", "", "A,B,C");
        }


        [TestMethod]
        public void DataLinkingSubstanceConversionsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceConversions, @"ResidueDefinitionsTests/ResidueDefinitionsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.ResidueDefinitions);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.Compounds);
            CollectionAssert.AreEqual(new[] { "A", "B" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.ResidueDefinitions);
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceConversions, ScopingType.Compounds, 3, "A,B", "C", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B");
        }
    }
}
