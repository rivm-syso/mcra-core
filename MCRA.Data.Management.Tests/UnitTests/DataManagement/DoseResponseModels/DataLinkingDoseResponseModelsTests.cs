using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingDoseResponseModelTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingDoseResponseModelsOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseModels);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseModels);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseModels, 3, "model1,model2", "model3", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.Responses, 3, "R1,R3", "R4", "R2");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.Compounds, 5, "S1,S2,S3,S4,S5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.DoseResponseExperiments, 2, "exp1,exp2", "", "");
        }

        [TestMethod]
        public void DataLinkingDoseResponseExperimentsOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseData);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 0, "", "", "Sys1,Sys2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 3, "R1,R2,R3", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseData);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseExperiments, 3, "exp1,exp2", "exp3", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Responses, 7, "R1,R2,R3", "R4,R5,R9,R10", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseExperiments, ScopingType.Compounds, 5, "S1,S2,S3,S4,S5", "", "");
        }

        [TestMethod]
        public void DataLinkingTestSystemsAndResponsesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple"),
                (ScopingType.TestSystems, @"DoseResponseTests/TestSystemsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Responses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 3, "R1,R2,R3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Responses, ScopingType.TestSystems, 2, "sys1,sys2", "", "Sys3");
        }

        [TestMethod]
        public void DataLinkingDoseResponseModelsFilterSubstancesResponsesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple"),
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsForFiltering")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseModels);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseModels);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseModels, 28,
                "drm15,drm16,drm19,drm22,drm23,drm26",
                "drm01,drm02,drm03,drm04,drm05,drm06,drm07,drm08,drm09,drm10,drm11,drm12,drm13,drm14,drm17,drm18,drm20,drm21,drm24,drm25,drm27,drm28",
                "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.Responses, 2, "R2", "R1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.Compounds, 4, "A,B", "C,D", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.DoseResponseExperiments, 2, "exp1,exp2", "", "");
        }

        [TestMethod]
        public void DataLinkingDoseResponseModelsFilterSubstancesResponsesExperimentsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseModels, @"DoseResponseTests/DoseResponseModelsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["a", "b"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);
            _rawDataProvider.SetFilterCodes(ScopingType.DoseResponseExperiments, ["Exp1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.DoseResponseModels);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DoseResponseModels);
            AssertDataReadingSummaryRecord(report, ScopingType.DoseResponseModels, 28,
                "drm15,drm16,drm19",
                "drm01,drm02,drm03,drm04,drm05,drm06,drm07,drm08,drm09,drm10,drm11,drm12,drm13,drm14,drm17,drm18,drm20,drm21,drm22,drm23,drm24,drm25,drm26,drm27,drm28",
                "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.Responses, 2, "R2", "R1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.Compounds, 4, "A,B", "C,D", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DoseResponseModels, ScopingType.DoseResponseExperiments, 2, "exp1", "exp2", "");
        }
    }
}
