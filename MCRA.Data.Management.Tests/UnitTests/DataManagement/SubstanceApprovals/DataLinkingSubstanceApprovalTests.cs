using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingSubstanceApprovalTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingSubstanceApprovalsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceApprovals, @"SubstanceApprovalsTests/SubstanceApprovalsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.SubstanceApprovals);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.SubstanceApprovals);
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceApprovals, ScopingType.Compounds, 5, "SubA,SubB,SubC,SubD,SubE","", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "SubA,SubB,SubC,SubD,SubE");
        }
    }
}
