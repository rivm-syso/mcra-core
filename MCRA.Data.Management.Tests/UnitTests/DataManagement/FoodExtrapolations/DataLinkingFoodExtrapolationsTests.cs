using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingFoodExtrapolationsTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingFoodExtrapolationsMatchedTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodExtrapolationsTests/FoodExtrapolationFoods"),
                (ScopingType.FoodExtrapolations, @"FoodExtrapolationsTests/FoodExtrapolations")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.FoodExtrapolations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FoodExtrapolations);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodExtrapolations, ScopingType.Foods, 5, "A,P,M,O,CF", "", "B,C");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 7, "A,P,M,O,CF,B,C", "", "");
        }

        [TestMethod]
        public void DataLinkingFoodExtrapolationsScopeTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodExtrapolations, @"FoodExtrapolationsTests/FoodExtrapolations")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.FoodExtrapolations);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.FoodExtrapolations);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodExtrapolations, ScopingType.Foods, 5, "A,P,M,O,CF", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "A,P,M,O,CF");
        }
    }
}
