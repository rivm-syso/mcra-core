using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingConsumptionsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingConsumptionsIndividualsOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Survey);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.DietaryIndividuals);
            Assert.AreEqual(5, scope.Count);
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4", "5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Survey);
            AssertDataReadingSummaryRecord(report, ScopingType.DietaryIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryIndividuals, ScopingType.FoodSurveys, 3, "S1,S2,S3", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
        }

        [TestMethod]
        public void DataLinkingConsumptionsIndividualsOnlyWithSurveyScopeTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, new[] { "s2" });
            _compiledLinkManager.LoadScope(SourceTableGroup.Survey);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.DietaryIndividuals);

            Assert.AreEqual(2, scope.Count);
            CollectionAssert.AreEqual(new[] { "3", "4" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Survey);
            AssertDataReadingSummaryRecord(report, ScopingType.DietaryIndividuals, 5, "3,4", "1,2,5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryIndividuals, ScopingType.FoodSurveys, 3, "S2", "S1,S3", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
        }

        [TestMethod]
        public void DataLinkingConsumptionsOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Survey);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.DietaryIndividuals);
            Assert.AreEqual(0, scope.Count);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Survey);
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.DietaryIndividuals, 6, "", "1,2,3,4,5,6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.Foods, 4, "", "f1,f2,f3,f4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "");
        }

        [TestMethod]
        public void DataLinkingConsumptionsIndividualsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Survey);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.DietaryIndividuals);
            Assert.AreEqual(5, scope.Count);
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4", "5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Survey);
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSurveys, 3, "S1,S2,S3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.DietaryIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryIndividuals, ScopingType.FoodSurveys, 3, "S1,S2,S3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.DietaryIndividuals, 6, "1,2,3,4,5", "6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.Foods, 4, "f1,f2,f3,f4", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4");
        }

        [TestMethod]
        public void DataLinkingConsumptionsIndividualsSurveyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Survey);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.FoodSurveys);
            Assert.AreEqual(3, scope.Count);
            CollectionAssert.AreEqual(new[] { "S1", "S2", "S3" }, scope.ToArray());

            scope = _compiledLinkManager.GetCodesInScope(ScopingType.DietaryIndividuals);
            Assert.AreEqual(5, scope.Count);
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4", "5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Survey);
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSurveys, 3, "S1,S2,S3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.DietaryIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryIndividuals, ScopingType.FoodSurveys, 3, "s1,s2,s3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.DietaryIndividuals, 6, "1,2,3,4,5", "6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.Foods, 4, "f1,f2,f3,f4", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4");
        }

        [TestMethod]
        public void DataLinkingConsumptionsIndividualsSurveyFilterFoodsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.Consumptions, @"ConsumptionsTests\Consumptions")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f2", "f3" });
            _compiledLinkManager.LoadScope(SourceTableGroup.Survey);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.FoodSurveys);
            Assert.AreEqual(3, scope.Count);
            CollectionAssert.AreEqual(new[] { "S1", "S2", "S3" }, scope.ToArray());

            scope = _compiledLinkManager.GetCodesInScope(ScopingType.DietaryIndividuals);
            Assert.AreEqual(5, scope.Count);
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4", "5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Survey);
            AssertDataReadingSummaryRecord(report, ScopingType.FoodSurveys, 3, "S1,S2,S3", "", "");
            AssertDataReadingSummaryRecord(report, ScopingType.DietaryIndividuals, 5, "1,2,3,4,5", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.DietaryIndividuals, ScopingType.FoodSurveys, 3, "S1,S2,S3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.DietaryIndividuals, 6, "1,2,3,4,5", "6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.Consumptions, ScopingType.Foods, 4, "f2,f3", "f1,f4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f2,f3");
        }
    }
}
