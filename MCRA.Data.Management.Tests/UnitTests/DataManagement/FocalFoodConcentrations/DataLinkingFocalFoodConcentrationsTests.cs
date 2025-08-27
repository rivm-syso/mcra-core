using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingFocalFoodConcentrationsTests : LinkTestsBase {
        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
        }

        [TestMethod]
        public void DataLinkingFocalFoodsFoodAndAnalysisSamplesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.FocalFoods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FocalFoods);
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");

            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodAnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");
        }

        [TestMethod]
        public void DataLinkingFocalFoodsFoodAndMissingAnalysisSamplesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesMissing"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.FocalFoods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FocalFoods);
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethods, 3, "Am1,Am2,Am3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, 2, "AS1,AS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodSamples, 2, "FS1,FS4", "", "FS2,FS3");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodAnalyticalMethods, 2, "Am2,Am3", "", "Am1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");
        }

        [TestMethod]
        public void DataLinkingFocalFoodsAnalyticalMethodCompoundsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.FocalFoods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FocalFoods);
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4,Am5");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.FocalFoodAnalyticalMethods, 5, "Am1,Am2,Am3,Am4,Am5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,q,r,s,t,z", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodAnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "Am4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,q,r,s,t,z");
        }

        [TestMethod]
        public void DataLinkingFocalFoodsAnalyticalMethodCompoundsFilterCompoundsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.FocalFoods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FocalFoods);
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4,Am5");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.FocalFoodAnalyticalMethods, 5, "Am1,Am2,Am3,Am4,Am5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,s", "q,r,t,z", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodAnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "Am4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,s");
        }

        [TestMethod]
        public void DataLinkingFocalFoodsAnalyticalMethodCompoundsFilterFoodsCompoundsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["A"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.FocalFoods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FocalFoods);
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.FocalFoodAnalyticalMethods, 5, "Am1,Am2,Am3,Am4", "Am5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,s", "q,r,t,z", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSamples, 4, "FS1,FS2", "FS3,FS4", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, 5, "AS1,AS2", "AS3,AS4,AS5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSamples, ScopingType.Foods, 3, "a", "b,d", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodSamples, 4, "FS1,FS2", "FS3,FS4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodAnalyticalMethods, 4, "Am1,Am2,Am3", "Am5", "Am4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,s");
        }

        [TestMethod]
        public void DataLinkingFocalFoodsAllDataSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.FocalFoodConcentrationsPerSample, @"FocalFoodsTests/ConcentrationsPerSampleSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.FocalFoods);
            _compiledLinkManager.LoadScope(SourceTableGroup.Compounds);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FocalFoods);
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethods, 3, "Am1,Am2,Am3", "", "Am4,Am5");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.FocalFoodAnalyticalMethods, 5, "Am1,Am2,Am3,Am4,Am5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodAnalyticalMethodCompounds, ScopingType.Compounds, 6, "p,q,r,s,t,z", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSamples, ScopingType.Foods, 3, "a,b,d", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodSamples, 4, "FS1,FS2,FS3,FS4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodSampleAnalyses, ScopingType.FocalFoodAnalyticalMethods, 4, "Am1,Am2,Am3,Am5", "", "Am4");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodConcentrationsPerSample, ScopingType.FocalFoodSampleAnalyses, 5, "AS1,AS2,AS3,AS4,AS5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FocalFoodConcentrationsPerSample, ScopingType.Compounds, 6, "p,q,r,s,t,z", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "a,b,d");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "p,q,r,s,z,t");
        }
    }
}
