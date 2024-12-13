using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingDoseResponseDataTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingDoseResponseExperimentsFilterSubstancesResponsesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests\ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests\ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseData);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseExperiments, 36,
                "x10,x11,x14,x19,x20,x23",
                "x01,x02,x03,x04,x05,x06,x07,x08,x09,x12,x13,x15,x16,x17,x18,x21,x22,x24,x25,x26,x27,x28,x29,x30,x31,x32,x33,x34,x35,x36",
                "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Responses, 4, "R2", "R1,R3,R4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Compounds, 4, "A,B", "C,D", "");
        }

        [TestMethod]
        public void DataLinkingDoseResponseExperimentsFilterExperimentsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests\ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests\ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.DoseResponseExperiments, ["X05", "X09", "x11", "x26", "x50", "x55"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseData);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseExperiments, 36,
                "x05,x09,x11,x26",
                "x01,x02,x03,x04,x06,x07,x08,x10,x12,x13,x14,x15,x16,x17,x18,x19,x20,x21,x22,x23,x24,x25,x27,x28,x29,x30,x31,x32,x33,x34,x35,x36",
                "x50,x55");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Responses, 4, "R1,R2,R3", "R4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Compounds, 4, "A,B,C,D", "", "");
        }

        [TestMethod]
        public void DataLinkingDoseResponseExperimentsFilterResponsesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests\ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests\ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseData);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseExperiments, 36,
                "x10,x11,x12,x13,x14,x15,x16,x17,x18,x19,x20,x21,x22,x23,x24,x25,x26,x27",
                "x01,x02,x03,x04,x05,x06,x07,x08,x09,x28,x29,x30,x31,x32,x33,x34,x35,x36",
                "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Responses, 4, "R2", "R1,R3,R4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Compounds, 4, "A,B,C,D", "", "");
        }

        [TestMethod]
        public void DataLinkingDoseResponseExperimentsFilterSubstancesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests\ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests\ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseData);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseExperiments, 36,
                "x01,x02,x05,x10,x11,x14,x19,x20,x23,x28,x29,x32",
                "x03,x04,x06,x07,x08,x09,x12,x13,x15,x16,x17,x18,x21,x22,x24,x25,x26,x27,x30,x31,x33,x34,x35,x36",
                "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Responses, 4, "R1,R2,R3", "R4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Compounds, 4, "A,B", "C,D", "");
        }
    }
}
