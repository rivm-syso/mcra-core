using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingOccurrencePatternsTests : LinkTestsBase {

        [TestMethod]
        public void DataLinkingOccurrencePatternsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.AgriculturalUse);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.OccurrencePatterns);
            CollectionAssert.AreEqual(new[] { "au1", "au2", "au3", "au4", "au5", "au6", "au7", "au8" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AgriculturalUse);
            AssertDataReadingSummaryRecord(report, ScopingType.OccurrencePatterns, 8, "au1,au2,au3,au4,au5,au6,au7,au8", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatterns, ScopingType.Foods, 4, "f1,f2,f3,f4", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4");
        }


        [TestMethod]
        public void DataLinkingOccurrencePatternsSimpleFoodsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AgriculturalUse);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.OccurrencePatterns);
            CollectionAssert.AreEqual(new[] { "au3", "au4", "au5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AgriculturalUse);
            AssertDataReadingSummaryRecord(report, ScopingType.OccurrencePatterns, 8, "au3,au4,au5", "au1,au2,au6,au7,au8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatterns, ScopingType.Foods, 4, "f2", "f1,f3,f4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f2");
        }

        [TestMethod]
        public void DataLinkingOccurrencePatternsCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.AgriculturalUse);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.OccurrencePatterns);
            CollectionAssert.AreEqual(new[] { "au1", "au2", "au3", "au4", "au5", "au6", "au7", "au8" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AgriculturalUse);
            AssertDataReadingSummaryRecord(report, ScopingType.OccurrencePatterns, 8, "au1,au2,au3,au4,au5,au6,au7,au8", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatterns, ScopingType.Foods, 4, "f1,f2,f3,f4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.OccurrencePatterns, 9, "au1,au2,au3,au4,au6,au7,au8", "au9,au10", "au5");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.Compounds, 5, "a,b,c,d,e", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,d,e");
        }

        [TestMethod]
        public void DataLinkingOccurrencePatternsCompoundsFoodsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AgriculturalUse);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.OccurrencePatterns);
            CollectionAssert.AreEqual(new[] { "au3", "au4", "au5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AgriculturalUse);
            AssertDataReadingSummaryRecord(report, ScopingType.OccurrencePatterns, 8, "au3,au4,au5", "au1,au2,au6,au7,au8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatterns, ScopingType.Foods, 4, "f2", "f1,f3,f4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.OccurrencePatterns, 9, "au3,au4", "au1,au2,au6,au7,au8,au9,au10", "au5");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.Compounds, 5, "a,b,c,d", "e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,d");
        }

        [TestMethod]
        public void DataLinkingOccurrencePatternsCompoundsFoodsCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["b", "c"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AgriculturalUse);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.OccurrencePatterns);
            CollectionAssert.AreEqual(new[] { "au3", "au4", "au5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AgriculturalUse);
            AssertDataReadingSummaryRecord(report, ScopingType.OccurrencePatterns, 8, "au3,au4,au5", "au1,au2,au6,au7,au8", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatterns, ScopingType.Foods, 4, "f2", "f1,f3,f4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.OccurrencePatterns, 9, "au3,au4", "au1,au2,au6,au7,au8,au9,au10", "au5");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.Compounds, 5, "b,c", "a,d,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }

        [TestMethod]
        public void DataLinkingOccurrencePatternsCompoundsCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["b", "c"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AgriculturalUse);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.OccurrencePatterns);
            CollectionAssert.AreEqual(new[] { "au1", "au2", "au3", "au4", "au5", "au6", "au7", "au8" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AgriculturalUse);
            AssertDataReadingSummaryRecord(report, ScopingType.OccurrencePatterns, 8, "au1,au2,au3,au4,au5,au6,au7,au8", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatterns, ScopingType.Foods, 4, "f1,f2,f3,f4", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.OccurrencePatterns, 9, "au1,au2,au3,au4,au6,au7,au8", "au9,au10", "au5");
            AssertDataLinkingSummaryRecord(report, ScopingType.OccurrencePatternsHasCompounds, ScopingType.Compounds, 5, "b,c", "a,d,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Foods);
            AssertDataReadingSummaryRecord(report, ScopingType.Foods, 0, "", "", "f1,f2,f3,f4");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }
    }
}
