using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    [TestClass]
    public class DataLinkingEffectRepresentationsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingEffectRepresentationsOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.EffectRepresentations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.EffectRepresentations);
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Effects, 6, "", "Eff1,Eff2,eff3,eff4,eff5,eff6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Responses, 4, "", "R1,R2,R3,R4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 0, "", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 0, "", "", "");
        }

        [TestMethod]
        public void DataLinkingEffectRepresentationsOnlyWithResponsesScopeTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Effects);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.TestSystems);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Responses);
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.EffectRepresentations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.EffectRepresentations);
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Effects, 6, "", "Eff1,Eff2,eff3,eff4,eff5,eff6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Responses, 4, "", "R1,R2,R3,R4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 0, "", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 0, "", "", "");
        }

        [TestMethod]
        public void DataLinkingEffectRepresentationsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.EffectRepresentations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.EffectRepresentations);
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Effects, 6, "Eff1,Eff2,eff3,eff4,eff5", "eff6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Responses, 4, "R1,R2,R3", "R4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4,eff5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 3, "R1,R2,R3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Responses, ScopingType.TestSystems, 2, "sys1,sys2", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 0, "", "", "sys1,sys2");
        }


        [TestMethod]
        public void DataLinkingEffectRepresentationsFilterEffectsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.EffectRepresentations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.EffectRepresentations);
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Effects, 6, "Eff1", "Eff2,eff3,eff4,eff5,eff6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Responses, 4, "R1,R2,R3", "R4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 3, "R1,R2,R3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Responses, ScopingType.TestSystems, 2, "sys1,sys2", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 0, "", "", "sys1,sys2");
        }

        [TestMethod]
        public void DataLinkingEffectRepresentationsFilterResponsesSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.EffectRepresentations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.EffectRepresentations);
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Effects, 6, "Eff2,eff3,eff4", "Eff1,eff5,eff6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Responses, 4, "R2", "R1,R3,R4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff2,eff3,eff4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 3, "R2", "R1,R3", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Responses, ScopingType.TestSystems, 2, "sys1", "sys2", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 0, "", "", "sys1");
        }

        [TestMethod]
        public void DataLinkingEffectRepresentationsFilterTestsystemsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple"),
                (ScopingType.TestSystems, @"DoseResponseTests/TestSystemsSimple"),
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.TestSystems, ["sys1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.EffectRepresentations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.EffectRepresentations);
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Effects, 6, "Eff1,Eff2,eff3,eff4,eff5", "eff6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Responses, 4, "R1,R2", "R3,R4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4,eff5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 3, "R1,R2", "R3", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Responses, ScopingType.TestSystems, 2, "sys1", "sys2", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 3, "sys1", "sys2,sys3", "");
        }

        [TestMethod]
        public void DataLinkingEffectRepresentationsFilterAllTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple"),
                (ScopingType.TestSystems, @"DoseResponseTests/TestSystemsSimple"),
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests/EffectRepresentationsSimple"),
                (ScopingType.Effects, @"EffectsTests/EffectsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.TestSystems, ["sys1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff2", "Eff5"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.EffectRepresentations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.EffectRepresentations);
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Effects, 6, "Eff2,eff5", "eff1,eff3,eff4,eff6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRepresentations, ScopingType.Responses, 4, "R2", "R1,R3,R4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 4, "Eff2", "eff1,eff3,eff4", "eff5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Responses);
            AssertDataReadingSummaryRecord(report, ScopingType.Responses, 3, "R2", "R1,R3", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Responses, ScopingType.TestSystems, 2, "sys1", "sys2", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.TestSystems);
            AssertDataReadingSummaryRecord(report, ScopingType.TestSystems, 3, "sys1", "sys2,sys3", "");
        }
    }
}
