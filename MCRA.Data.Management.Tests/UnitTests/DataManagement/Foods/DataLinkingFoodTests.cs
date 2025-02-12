using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    [TestClass]
    public class DataLinkingFoodTests : LinkTestsBase {

        /// <summary>
        /// Test reading of simple foods data source.
        /// </summary>
        [TestMethod]
        public void DataLinkingFood_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests/FoodsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 3, "A,B,C", "", "");
        }

        /// <summary>
        /// Test reading of foods data source with scope filter.
        /// </summary>
        [TestMethod]
        public void DataLinkingFood_TestFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests/FoodsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["A", "C"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 3, "A,C", "B", "");
        }

        /// <summary>
        /// Test reading of two food data sources.
        /// </summary>
        [TestMethod]
        public void DataLinkingFood_TestMultiple() {
            _rawDataProvider.SetDataTables(1, (ScopingType.Foods, @"FoodsTests/FoodsSimple"));
            _rawDataProvider.SetDataTables(2, (ScopingType.Foods, @"FoodsTests/FoodsAdditional"));
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 7, "A,B,C,D,E,F,G", "", "");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 1, "A,B,C", "", "D,E,F,G");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 2, "D,E,F,G", "", "A,B,C");
        }

        /// <summary>
        /// Test reading of two food data sources with scope filter.
        /// </summary>
        [TestMethod]
        public void DataLinkingFood_TestMultipleFiltered() {
            _rawDataProvider.SetDataTables(1, (ScopingType.Foods, @"FoodsTests/FoodsSimple"));
            _rawDataProvider.SetDataTables(2, (ScopingType.Foods, @"FoodsTests/FoodsAdditional"));
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["A", "C", "E", "G", "xxx"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 7, "A,C,E,G", "B,D,F", "xxx");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 1, "A,C", "B", "E,G,xxx");
            AssertDataSourceReadingSummaryRecord(report, ScopingType.Foods, 2, "E,G", "D,F", "A,C,xxx");
        }

        [TestMethod]
        public void DataLinkingFood_TestHierarchiesMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodHierarchies, @"FoodsTests/FoodHierarchies")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "AP,A,F,W");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodHierarchies, ScopingType.Foods, 4, "AP,A,F,W", "", "");
        }

        [TestMethod]
        public void DataLinkingFood_TestHierarchiesMatchedFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodHierarchies, @"FoodsTests/FoodHierarchies")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["AP", "F"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "AP,F");
        }

        [TestMethod]
        public void DataLinkingFood_TestHierarchiesUnmatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodHierarchies, @"FoodsTests/FoodHierarchies")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodHierarchies, ScopingType.Foods, 4, "AP,F,A,W", "", "");
        }

        [TestMethod]
        public void DataLinkingFood_TestConsumptionQuantificationsMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodConsumptionQuantifications, @"FoodsTests/FoodConsumptionQuantifications")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "AP,A,F");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodConsumptionQuantifications, ScopingType.Foods, 3, "AP,A,F", "", "");
        }

        [TestMethod]
        public void DataLinkingFood_TestConsumptionQuantificationsUnmatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodConsumptionQuantifications, @"FoodsTests/FoodConsumptionQuantifications")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodConsumptionQuantifications, ScopingType.Foods, 3, "AP,A,F", "", "");
        }

        [TestMethod]
        public void DataLinkingFood_TestPropertiesMatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests/FoodsSimple"),
                (ScopingType.FoodProperties, @"FoodsTests/FoodsSimpleProperties")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 3, "A,B,C", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodProperties, ScopingType.Foods, 3, "A,B,C", "", "");
        }

        [TestMethod]
        public void DataLinkingFood_TestPropertiesUnmatched() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodProperties, @"FoodsTests/FoodsSimpleProperties")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataLinkingSummaryRecord(report, ScopingType.FoodProperties, ScopingType.Foods, 3, "A,B,C", "", "");
        }

        [TestMethod]
        public void DataLinkingFood_TestFromConsumptions() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetDataTables(
                (ScopingType.Consumptions, @"FoodsTests/FoodConsumptionsSimple"),
                (ScopingType.DietaryIndividuals, @"FoodsTests/IndividualsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "A,B,C");
        }

        [TestMethod]
        public void DataLinkingFood_TestFromConsumptionsFiltered() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetDataTables(
                (ScopingType.Consumptions, @"FoodsTests/FoodConsumptionsSimple"),
                (ScopingType.DietaryIndividuals, @"FoodsTests/IndividualsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["B"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "B");
        }

        [TestMethod]
        public void DataLinkingFood_TestFromProcessingFactors() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"FoodsTests/ProcessingFactorsSimple")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "A,B,C");
        }

        [TestMethod]
        public void DataLinkingFood_TestFromProcessingFactorsFiltered() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"FoodsTests/ProcessingFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["A", "B"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "A,B");
        }

        [TestMethod]
        public void DataLinkingFood_TestWithSameFoodsFromConsumptionsAndProcessingFactors() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetDataTables(
                (ScopingType.DietaryIndividuals, @"FoodsTests/IndividualsSimple"),
                (ScopingType.Consumptions, @"FoodsTests/FoodConsumptionsSimple"),
                (ScopingType.ProcessingFactors, @"FoodsTests/ProcessingFactorsSimple")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "A,B,C");
        }

        [TestMethod]
        public void DataLinkingFood_TestFoodEx2FoodsAndFacets() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FacetDescriptors, @"FoodsTests/FoodEx2FacetDescriptors"),
                (ScopingType.Foods, @"FoodsTests/FoodEx2Foods"),
                (ScopingType.Facets, @"FoodsTests/FoodEx2Facets")
            );
            _compiledLinkManager.LoadScope(SourceTableGroup.Foods);
            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Facets, 3, "F01,F02,F03", "", "");
        }
    }
}
