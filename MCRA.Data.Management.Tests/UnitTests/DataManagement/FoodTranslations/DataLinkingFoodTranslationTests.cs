using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingFoodTranslationTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingFoodTranslation_TestMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodTranslationTests/FoodTranslationFoods"),
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslations")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.FoodTranslations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FoodTranslations);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 4, "AP,A,F,W", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 4, "AP,A,F,W", "", "");
        }

        [TestMethod]
        public void DataLinkingFoodTranslation_TestScope() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslations")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.FoodTranslations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FoodTranslations);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 4, "AP,A,F,W", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "AP,A,F,W");
        }

        [TestMethod]
        public void DataLinkingFoodTranslation_TestMultipleMatched() {
            _rawDataProvider.SetDataTables(
                1,
                (ScopingType.Foods, @"FoodTranslationTests/FoodTranslationFoods"),
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslations")
            );
            _rawDataProvider.SetDataTables(
                2,
                (ScopingType.Foods, @"FoodTranslationTests/FoodTranslationFoodsAdditional"),
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslationsAdditional")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.FoodTranslations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FoodTranslations);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 8, "AP,A,F,W,FM,B,C,P", "", "");
            AssertDataSourceLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 1, "AP,A,F,W", "", "FM,B,C,P");
            AssertDataSourceLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 2, "FM,B,C,P", "", "AP,A,F,W");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 8, "AP,A,F,W,FM,B,C,P", "", "");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 1, "AP,A,F,W", "", "FM,B,C,P");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 2, "FM,B,C,P", "", "AP,A,F,W");
        }

        [TestMethod]
        public void DataLinkingFoodTranslation_TestMultipleFiltered() {
            _rawDataProvider.SetDataTables(
                1,
                (ScopingType.Foods, @"FoodTranslationTests/FoodTranslationFoods"),
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslations")
            );
            _rawDataProvider.SetDataTables(
                2,
                (ScopingType.Foods, @"FoodTranslationTests/FoodTranslationFoodsAdditional"),
                (ScopingType.FoodTranslations, @"FoodTranslationTests/FoodTranslationsAdditional")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["AP", "A", "F", "xxx"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.FoodTranslations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 8, "AP,A,F", "W,FM,B,C,P", "xxx");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 1, "AP,A,F", "W", "xxx");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 2, "", "FM,B,C,P", "AP,A,F,xxx");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FoodTranslations);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 8, "AP,A,F", "W,FM,B,C,P", "xxx");
            AssertDataSourceLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 1, "AP,A,F", "W", "xxx");
            AssertDataSourceLinkingSummaryRecord(report, ScopingType.FoodTranslations, ScopingType.Foods, 2, "", "FM,B,C,P", "AP,A,F,xxx");
        }
    }
}
