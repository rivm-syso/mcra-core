using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    [TestClass]
    public class DataLinkingPbkModelsTests : LinkTestsBase {

        private static string GetOtherModelAliases(params string[] codes) {
            var aliases = MCRAKineticModelDefinitions.Definitions
                .SelectMany(r => r.Value.Aliases)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                .Except(codes, StringComparer.OrdinalIgnoreCase)
                .Select(r => r.ToLower());
            var result = string.Join(",", aliases);
            return result;
        }

        [TestMethod]
        public void DataLinkingKineticModelInstancesSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests\KineticModelInstancesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.PbkModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.KineticModelInstances);
            CollectionAssert.AreEqual(new[] { "km01", "km02", "km03", "km04", "km05", "km06", "km07", "km08", "km09", "km10" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.PbkModels);
            AssertDataReadingSummaryRecord(report, ScopingType.KineticModelInstances, 12, "km01,km02,km03,km04,km05,km06,km07,km08,km09,km10", "km11,km12", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.Compounds, 7, "A,B,C,D,E", "R,S", "");
            //special case: Kinetic Model Definitions have no ScopingType, use 'Unknown' here
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.KineticModelDefinitions, 2, "cosmosv4", "xx", GetOtherModelAliases("cosmosv4"));

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E");
        }

        [TestMethod]
        public void DataLinkingKineticModelInstancesSimpleFilterInstanceTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests\KineticModelInstancesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.KineticModelInstances, ["km04", "km06", "km11", "km20", "km99"]);
            _compiledLinkManager.LoadScope(SourceTableGroup.PbkModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.KineticModelInstances);
            CollectionAssert.AreEqual(new[] { "km04", "km06", "km11" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.PbkModels);
            AssertDataReadingSummaryRecord(report, ScopingType.KineticModelInstances, 12, "km04,km06,km11", "km01,km02,km03,km05,km07,km08,km09,km10,km12", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.Compounds, 7, "A,E", "B,C,D,R,S", "");
            //special case: Kinetic Model Definitions have no ScopingType, use 'Unknown' here
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.KineticModelDefinitions, 2, "cosmosv4", "xx", GetOtherModelAliases("cosmosv4"));

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,E");
        }


        [TestMethod]
        public void DataLinkingKineticModelInstancesSimpleSubstanceFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests\KineticModelInstancesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.PbkModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.KineticModelInstances);
            CollectionAssert.AreEqual(new[] { "km02", "km03", "km07", "km08" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.PbkModels);
            AssertDataReadingSummaryRecord(report, ScopingType.KineticModelInstances, 12, "km02,km03,km07,km08", "km01,km04,km05,km06,km09,km10,km11,km12", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.Compounds, 7, "B,C", "A,D,E,R,S", "");
            //special case: Kinetic Model Definitions have no ScopingType, use 'Unknown' here
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.KineticModelDefinitions, 2, "cosmosv4", "xx", GetOtherModelAliases("cosmosv4"));

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "B,C");
        }

        [TestMethod]
        public void DataLinkingKineticModelInstanceParametersSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests\KineticModelInstancesSimple"),
                (ScopingType.KineticModelInstanceParameters, @"KineticModelsTests\KineticModelInstanceParametersSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.PbkModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.KineticModelInstances);
            CollectionAssert.AreEqual(new[] { "km01", "km02", "km03", "km04", "km05", "km06", "km07", "km08", "km09", "km10" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.PbkModels);
            AssertDataReadingSummaryRecord(report, ScopingType.KineticModelInstances, 12, "km01,km02,km03,km04,km05,km06,km07,km08,km09,km10", "km11,km12", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.Compounds, 7, "A,B,C,D,E", "R,S", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstances, ScopingType.KineticModelDefinitions, 2, "cosmosv4", "xx", GetOtherModelAliases("cosmosv4"));
            AssertDataLinkingSummaryRecord(report, ScopingType.KineticModelInstanceParameters, ScopingType.KineticModelInstances, 8, "km01,km03,km06,km07", "km11,km15,km17,km22", "km02,km04,km05,km08,km09,km10");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "A,B,C,D,E");
        }
    }
}
