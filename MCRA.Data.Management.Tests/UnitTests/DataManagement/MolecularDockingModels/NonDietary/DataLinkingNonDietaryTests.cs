using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingNonDietaryTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingNonDietarySurveysOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests\NonDietarySurveys")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietary);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietary);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietarySurveys, 2, "s1,s2", "", "");
        }

        [TestMethod]
        public void DataLinkingNonDietarySurveysOnlyScopeTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.NonDietary);
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, new[] { "s2" });
            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietary);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietary);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietarySurveys, 0, "", "", "");
        }

        [TestMethod]
        public void DataLinkingNonDietarySurveysOnlyWithSurveyScopeTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests\NonDietarySurveys")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, new[] { "s2" });
            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietary);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietary);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietarySurveys, 2, "s2", "s1", "");
        }

        [TestMethod]
        public void DataLinkingNonDietarySurveysAndExposuresTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests\NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests\NonDietaryExposures")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietary);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietary);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietarySurveys, 2, "s1,s2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.NonDietaryExposures, ScopingType.NonDietarySurveys, 4, "s1,s2", "s4,s5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.NonDietaryExposures, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }

        [TestMethod]
        public void DataLinkingNonDietarySurveysAndExposuresFilterCompoundsTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests\NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests\NonDietaryExposures")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietary);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietary);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietarySurveys, 2, "s1,s2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.NonDietaryExposures, ScopingType.NonDietarySurveys, 4, "s1,s2", "s4,s5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.NonDietaryExposures, ScopingType.Compounds, 6, "B,C", "A,D,E,F", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }

        [TestMethod]
        public void DataLinkingNonDietarySurveysAndExposuresFilterSurveyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests\NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests\NonDietaryExposures")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, new[] { "s2" });

            _compiledLinkManager.LoadScope(SourceTableGroup.NonDietary);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.NonDietary);
            AssertDataReadingSummaryRecord(report, ScopingType.NonDietarySurveys, 2, "s2", "s1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.NonDietaryExposures, ScopingType.NonDietarySurveys, 4, "s2", "s1,s4,s5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.NonDietaryExposures, ScopingType.Compounds, 6, "A,C,E,F", "B,D", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,C,E,F");
        }
    }
}
