using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingPointsOfDepartureTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingPointsOfDepartureSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardDoses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardDoses);
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Effects, 4, "Eff1,Eff2,eff3,eff4", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }


        [TestMethod]
        public void DataLinkingPointsOfDepartureSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff3"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardDoses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardDoses);
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Compounds, 6, "A,B,C,D,E", "F", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Effects, 4, "Eff1,Eff3", "Eff2,eff4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1,eff3");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E");
        }

        [TestMethod]
        public void DataLinkingPointsOfDepartureSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardDoses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardDoses);
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Effects, 4, "Eff1,Eff2,eff3", "eff4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }

        [TestMethod]
        public void DataLinkingPointsOfDepartureFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.HazardDoses);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.HazardDoses);
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.PointsOfDeparture, ScopingType.Effects, 4, "Eff1", "Eff2,eff3,eff4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }
    }
}
