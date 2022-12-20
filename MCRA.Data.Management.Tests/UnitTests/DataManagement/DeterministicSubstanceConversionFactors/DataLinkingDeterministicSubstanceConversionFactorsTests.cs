using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingDeterministicSubstanceConversionFactorsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingDeterministicSubstanceConversionFactors_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DeterministicSubstanceConversionFactors, @"DeterministicSubstanceConversionFactorsTests\DeterministicSubstanceConversionFactorsSimple"),
                (ScopingType.Compounds, @"SubstancesTests\SubstancesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.DeterministicSubstanceConversionFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DeterministicSubstanceConversionFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.DeterministicSubstanceConversionFactors, ScopingType.Compounds, 3, "A,B,C", "", "D,E");
        }

        [TestMethod]
        public void DataLinkingDeterministicSubstanceConversionFactors_TestAutoScopeSubstances() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DeterministicSubstanceConversionFactors, @"DeterministicSubstanceConversionFactorsTests\DeterministicSubstanceConversionFactorsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.DeterministicSubstanceConversionFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DeterministicSubstanceConversionFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.DeterministicSubstanceConversionFactors, ScopingType.Compounds, 3, "A,B,C", "", "");
        }

        [TestMethod]
        public void DataLinkingDeterministicSubstanceConversionFactors_TestScopeSubstances() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DeterministicSubstanceConversionFactors, @"DeterministicSubstanceConversionFactorsTests\DeterministicSubstanceConversionFactorsSimple"),
                (ScopingType.Compounds, @"SubstancesTests\SubstancesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            _compiledLinkManager.LoadScope(SourceTableGroup.DeterministicSubstanceConversionFactors);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.DeterministicSubstanceConversionFactors);
            AssertDataLinkingSummaryRecord(report, ScopingType.DeterministicSubstanceConversionFactors, ScopingType.Compounds, 3, "B,C", "A", "");
        }
    }
}
