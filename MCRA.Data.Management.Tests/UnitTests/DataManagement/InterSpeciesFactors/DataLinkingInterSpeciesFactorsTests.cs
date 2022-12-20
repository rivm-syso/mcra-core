using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingInterSpeciesFactorsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingInterSpeciesFactorsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.InterSpeciesFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.InterSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Compounds, 6, "A,B,C,D,E,F", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Effects, 6, "Eff1,Eff2,eff3,eff4,eff5,eff6", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4,eff5,eff6");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E,F");
        }

        [TestMethod]
        public void DataLinkingInterSpeciesFactorsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "Eff1" });

            _compiledLinkManager.LoadScope(SourceTableGroup.InterSpeciesFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.InterSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Compounds, 6, "", "A,B,C,D,E,F", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Effects, 6, "Eff1", "Eff2,eff3,eff4,eff5,eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "");
        }

        [TestMethod]
        public void DataLinkingInterSpeciesFactorsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.InterSpeciesFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.InterSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Effects, 6, "Eff1,Eff2,eff3,eff4,eff5", "eff6", "");


            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2,eff3,eff4,eff5");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }

        [TestMethod]
        public void DataLinkingInterSpeciesModelParametersFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "Eff1" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.InterSpeciesFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.InterSpeciesFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Compounds, 6, "b,c", "a,d,e,f", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.InterSpeciesModelParameters, ScopingType.Effects, 6, "Eff1", "Eff2,eff3,eff4,eff5,eff6", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }
    }
}
