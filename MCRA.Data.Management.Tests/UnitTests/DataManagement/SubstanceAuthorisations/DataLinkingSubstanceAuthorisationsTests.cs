using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingSubstanceAuthorisationsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingSubstanceAuthorisationsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.AuthorisedUses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AuthorisedUses);
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Foods, 6, "f1,f2,f3,f4,f5,f6", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5,f6");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }

        [TestMethod]
        public void DataLinkingSubstanceAuthorisationsSimpleFoodsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AuthorisedUses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AuthorisedUses);
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Foods, 6, "f1", "f2,f3,f4,f5,f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Compounds, 6, "B", "A,C,D,E,F", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B");
        }

        [TestMethod]
        public void DataLinkingSubstanceAuthorisationsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AuthorisedUses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AuthorisedUses);
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Foods, 6, "f1,f2,f3,f4,f5", "f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4,f5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }

        [TestMethod]
        public void DataLinkingSubstanceAuthorisationsFilterFoodsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests/AuthorisedUsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AuthorisedUses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AuthorisedUses);
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Foods, 6, "f1", "f2,f3,f4,f5,f6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SubstanceAuthorisations, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }
    }
}
