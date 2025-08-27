using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingHazardCharacterisationsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingHazardCharacterisationsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardCharacterisations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardCharacterisations);
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Effects, 4, "Eff1,Eff2,eff3,eff4", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");

            var records = _compiledLinkManager.GetAllScopeEntities(ScopingType.HazardCharacterisations);
            Assert.AreEqual(13, records.Count);
        }


        [TestMethod]
        public void DataLinkingHazardCharacterisationsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff3"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardCharacterisations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardCharacterisations);
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Compounds, 6, "A,B,C,D,E", "F", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Effects, 4, "Eff1,Eff3", "Eff2,eff4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1,eff3");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E");
        }

        [TestMethod]
        public void DataLinkingHazardCharacterisationsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardCharacterisations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardCharacterisations);
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Effects, 4, "Eff1,Eff2,eff3", "eff4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }

        [TestMethod]
        public void DataLinkingHazardCharacterisationsFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardCharacterisations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardCharacterisations);
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.HazardCharacterisations, ScopingType.Effects, 4, "Eff1", "Eff2,eff3,eff4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }
    }
}
