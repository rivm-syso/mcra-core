using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingHumanMonitoringDataTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingHumanMonitoringDataIndividualsOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s1,s2,s3", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
        }

        [TestMethod]
        public void DataLinkingHumanMonitoringDataIndividualsOnlyWithSurveyScopeTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, new[] { "s2" });
            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSurveys, 0, "", "", "s2");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "3,4", "1,2,5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s2", "s1,s3", "");
        }

        [TestMethod]
        public void DataLinkingHumanMonitoringDataIndividualsOnlyWithSurveyDataTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests\AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSurveys, @"HumanMonitoringDataTests\HumanMonitoringSurveys")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSurveys, 2, "s1,s2", "", "s3");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s1,s2,s3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s1,s2,s3", "", "");
        }

        [TestMethod]
        public void DataLinkingHumanMonitoringSamplesIndividualsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests\AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests\HumanMonitoringSamplesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s1,s2,s3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSamples, 5, "hs1,hs2,hs3,hs4", "hs5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSamples, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4", "8", "5");
        }

        [TestMethod]
        public void DataLinkingHumanMonitoringSamplesIndividualsSurveyFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests\AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests\HumanMonitoringSamplesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, new[] { "s2" });

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSurveys, 0, "", "", "s2");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "3,4", "1,2,5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s2", "s1,s3", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSamples, 5, "hs3,hs4", "hs1,hs2,hs5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSamples, ScopingType.HumanMonitoringIndividuals, 5, "3,4", "1,2,8", "");
        }

        [TestMethod]
        public void DataLinkingHumanMonitoringSampleAnalysesIndividualsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests\AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests\HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests\HumanMonitoringSampleAnalysesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s1,s2,s3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSamples, 5, "hs1,hs2,hs3,hs4", "hs5", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, 4, "as1,as2,as4", "as9", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSamples, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4", "8", "5");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringSamples, 4, "hs1,hs2,hs4", "hs9", "hs3");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringAnalyticalMethods, 4, "am1,am2,am3", "am9", "am4");
        }

        [TestMethod]
        public void DataLinkingHumanMonitoringSampleAnalysesIndividualsSurveyFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests\AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests\HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests\HumanMonitoringSampleAnalysesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, new[] { "s2" });

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSurveys, 0, "", "", "s2");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "3,4", "1,2,5", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSamples, 5, "hs3,hs4", "hs1,hs2,hs5", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, 4, "as4", "as1,as2,as9", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s2", "s1,s3", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSamples, ScopingType.HumanMonitoringIndividuals, 5, "3,4", "1,2,8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringSamples, 4, "hs4", "hs1,hs2,hs9", "hs3");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringAnalyticalMethods, 4, "am1,am2,am3", "am9", "am4");
        }


        [TestMethod]
        public void DataLinkingHumanMonitoringSampleAnalysesConcentrationsIndividualsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests\AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests\HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests\HumanMonitoringSampleAnalysesSimple"),
                (ScopingType.HumanMonitoringSampleConcentrations, @"HumanMonitoringDataTests\HumanMonitoringSampleConcentrations")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s1,s2,s3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSamples, 5, "hs1,hs2,hs3,hs4", "hs5", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, 4, "as1,as2,as4", "as9", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s1,s2,s3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSamples, ScopingType.HumanMonitoringIndividuals, 5, "1,2,3,4", "8", "5");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringSamples, 4, "hs1,hs2,hs4", "hs9", "hs3");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringAnalyticalMethods, 4, "am1,am2,am3", "am9", "am4");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleConcentrations, ScopingType.HumanMonitoringSampleAnalyses, 5, "as1,as2,as4", "as3,as5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleConcentrations, ScopingType.Compounds, 6, "p,q,r,s,t", "z", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,q,r,s,t");
        }


        [TestMethod]
        public void DataLinkingHumanMonitoringSampleAnalysesConcentrationsIndividualsFilterSurveyCompoundsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests\AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests\HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests\HumanMonitoringSampleAnalysesSimple"),
                (ScopingType.HumanMonitoringSampleConcentrations, @"HumanMonitoringDataTests\HumanMonitoringSampleConcentrations")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "P", "S" });
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, new[] { "s2" });

            _compiledLinkManager.LoadScope(SourceTableGroup.HumanMonitoringData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HumanMonitoringData);
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSurveys, 0, "", "", "s2");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, 5, "3,4", "1,2,5", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSamples, 5, "hs3,hs4", "hs1,hs2,hs5", "");
            AssertDataReadingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, 4, "as4", "as1,as2,as9", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringIndividuals, ScopingType.HumanMonitoringSurveys, 3, "s2", "s1,s3", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSamples, ScopingType.HumanMonitoringIndividuals, 5, "3,4", "1,2,8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringSamples, 4, "hs4", "hs1,hs2,hs9", "hs3");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleAnalyses, ScopingType.HumanMonitoringAnalyticalMethods, 4, "am1,am2,am3", "am9", "am4");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleConcentrations, ScopingType.HumanMonitoringSampleAnalyses, 5, "as4", "as1,as2,as3,as5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HumanMonitoringSampleConcentrations, ScopingType.Compounds, 6, "p,s", "q,r,t,z", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,s");
        }
    }
}
