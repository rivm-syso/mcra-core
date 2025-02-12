using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingQsarMembershipModelsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingQsarMembershipModelsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.QsarMembershipModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.QsarMembershipModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.QsarMembershipModels);
            AssertDataReadingSummaryRecord(report, ScopingType.QsarMembershipModels, 3, "MD1,MD2,MD3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipModels, ScopingType.Effects, 2, "Eff1,Eff2", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2");
        }

        [TestMethod]
        public void DataLinkingQsarMembershipModelsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.QsarMembershipModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.QsarMembershipModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.QsarMembershipModels);
            AssertDataReadingSummaryRecord(report, ScopingType.QsarMembershipModels, 3, "MD1,MD3", "MD2", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipModels, ScopingType.Effects, 2, "Eff1", "Eff2", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");
        }

        [TestMethod]
        public void DataLinkingQsarMembershipScoresSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple"),
                (ScopingType.QsarMembershipScores, @"QsarMembershipModelsTests/QsarMembershipScoresSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.QsarMembershipModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.QsarMembershipModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.QsarMembershipModels);
            AssertDataReadingSummaryRecord(report, ScopingType.QsarMembershipModels, 3, "MD1,MD2,MD3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipModels, ScopingType.Effects, 2, "Eff1,Eff2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipScores, ScopingType.QsarMembershipModels, 5, "MD1,MD2,MD3", "MD4,MD5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipScores, ScopingType.Compounds, 5, "a,b,c,d", "e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,d");
        }

        [TestMethod]
        public void DataLinkingQsarMembershipScoresFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple"),
                (ScopingType.QsarMembershipScores, @"QsarMembershipModelsTests/QsarMembershipScoresSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B", "D"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.QsarMembershipModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.QsarMembershipModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.QsarMembershipModels);
            AssertDataReadingSummaryRecord(report, ScopingType.QsarMembershipModels, 3, "MD1,MD3", "MD2", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipModels, ScopingType.Effects, 2, "Eff1", "Eff2", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipScores, ScopingType.QsarMembershipModels, 5, "MD1,MD3", "MD2,MD4,MD5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.QsarMembershipScores, ScopingType.Compounds, 5, "a,b,d", "c,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,d");
        }
    }
}
