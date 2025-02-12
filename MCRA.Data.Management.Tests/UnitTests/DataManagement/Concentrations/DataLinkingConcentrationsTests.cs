using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingConcentrationsTests : LinkTestsBase {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
        }

        [TestMethod]
        public void DataLinkingConcentrations_FoodAndAnalysisSamplesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Concentrations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Concentrations);
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");

            AssertDataReadingSummaryRecord(report, ScopingType.SampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.AnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");
        }

        [TestMethod]
        public void DataLinkingConcentrations_FoodAndMissingAnalysisSamplesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesMissing"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests/AnalyticalMethodsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Concentrations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Concentrations);
            AssertDataReadingSummaryRecord(report, ScopingType.AnalyticalMethods, 3, "Am1,Am2,Am3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.SampleAnalyses, 2, "AS1,AS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.FoodSamples, 2, "FS1,FS4", "", "FS2,FS3");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.AnalyticalMethods, 2, "Am2,Am3", "", "Am1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");
        }

        [TestMethod]
        public void DataLinkingConcentrations_AnalyticalMethodCompoundsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests/AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests/AnalyticalMethodCompoundsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Concentrations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Concentrations);
            AssertDataReadingSummaryRecord(report, ScopingType.AnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4,Am5");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.AnalyticalMethods, 5, "Am1,Am2,Am3,Am4,Am5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,q,r,s,t,z", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.SampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.AnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "Am4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,q,r,s,t,z");
        }

        [TestMethod]
        public void DataLinkingConcentrations_AnalyticalMethodCompoundsFilterCompoundsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests/AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests/AnalyticalMethodCompoundsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.Concentrations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Concentrations);
            AssertDataReadingSummaryRecord(report, ScopingType.AnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4,Am5");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.AnalyticalMethods, 5, "Am1,Am2,Am3,Am4,Am5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,s", "q,r,t,z", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.SampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.AnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "Am4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,s");
        }

        [TestMethod]
        public void DataLinkingConcentrations_AnalyticalMethodCompoundsFilterFoodsCompoundsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests/AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests/AnalyticalMethodCompoundsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["A"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.Concentrations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Concentrations);
            AssertDataReadingSummaryRecord(report, ScopingType.AnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.AnalyticalMethods, 5, "Am1,Am2,Am3,Am4", "Am5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,s", "q,r,t,z", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSamples, 4, "FS1,FS2", "FS3,FS4", "");
            AssertDataReadingSummaryRecord(report, ScopingType.SampleAnalyses, 5, "AS1,AS2", "AS3,AS4,AS5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodSamples, ScopingType.Foods, 3, "a", "b,d", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.FoodSamples, 4, "FS1,FS2", "FS3,FS4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.AnalyticalMethods, 4, "Am1,Am2,Am3", "Am5", "Am4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,s");
        }

        [TestMethod]
        public void DataLinkingConcentrations_AllDataSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests/FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests/AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests/AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.ConcentrationsPerSample, @"ConcentrationsTests/ConcentrationsPerSampleSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Concentrations);
            _compiledLinkManager.LoadScope(SourceTableGroup.Compounds);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Concentrations);
            AssertDataReadingSummaryRecord(report, ScopingType.AnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4,Am5");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.AnalyticalMethods, 5, "Am1,Am2,Am3,Am4,Am5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,q,r,s,t,z", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.SampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.FoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.SampleAnalyses, ScopingType.AnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "Am4");
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationsPerSample, ScopingType.SampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ConcentrationsPerSample, ScopingType.Compounds, 6, "p,q,r,s,t,z", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,q,r,s,z,t");
        }
    }
}
