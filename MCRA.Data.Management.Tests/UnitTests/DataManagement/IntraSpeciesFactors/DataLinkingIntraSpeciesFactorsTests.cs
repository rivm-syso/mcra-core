using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingIntraSpeciesFactorsTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingIntraSpeciesFactorsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.IntraSpeciesModelParameters, @"IntraSpeciesFactorsTests/IntraSpeciesModelParametersSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.IntraSpeciesFactors);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.IntraSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Effects, 6, "Eff1,Eff2,eff3,eff4,eff5,eff6", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4,eff5,eff6");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }

        [TestMethod]
        public void DataLinkingIntraSpeciesFactorsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.IntraSpeciesModelParameters, @"IntraSpeciesFactorsTests/IntraSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.IntraSpeciesFactors);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.IntraSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Compounds, 6, "", "A,B,C,D,E,F", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Effects, 6, "Eff1", "Eff2,eff3,eff4,eff5,eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "");
        }

        [TestMethod]
        public void DataLinkingIntraSpeciesFactorsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.IntraSpeciesModelParameters, @"IntraSpeciesFactorsTests/IntraSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.IntraSpeciesFactors);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.IntraSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Effects, 6, "Eff1,Eff2,eff3,eff4,eff5", "eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4,eff5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }

        [TestMethod]
        public void DataLinkingIntraSpeciesModelParametersFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.IntraSpeciesModelParameters, @"IntraSpeciesFactorsTests/IntraSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.IntraSpeciesFactors);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.IntraSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.IntraSpeciesModelParameters, ScopingType.Effects, 6, "Eff1", "Eff2,eff3,eff4,eff5,eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }
    }
}
