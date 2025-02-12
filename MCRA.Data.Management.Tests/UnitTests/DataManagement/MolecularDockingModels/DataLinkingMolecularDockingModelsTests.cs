using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingMolecularDockingModelsTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingMolecularDockingModelsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.MolecularDockingModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.MolecularDockingModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MolecularDockingModels);
            AssertDataReadingSummaryRecord(report, ScopingType.MolecularDockingModels, 3, "MD1,MD2,MD3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularDockingModels, ScopingType.Effects, 2, "Eff1,Eff2", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2");
        }


        [TestMethod]
        public void DataLinkingMolecularDockingModelsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.MolecularDockingModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.MolecularDockingModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MolecularDockingModels);
            AssertDataReadingSummaryRecord(report, ScopingType.MolecularDockingModels, 3, "MD1,MD3", "MD2", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularDockingModels, ScopingType.Effects, 2, "Eff1", "Eff2", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");
        }

        [TestMethod]
        public void DataLinkingMolecularBindingEnergiesSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple"),
                (ScopingType.MolecularBindingEnergies, @"MolecularDockingModelsTests/MolecularBindingEnergiesSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.MolecularDockingModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.MolecularDockingModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MolecularDockingModels);
            AssertDataReadingSummaryRecord(report, ScopingType.MolecularDockingModels, 3, "MD1,MD2,MD3", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularDockingModels, ScopingType.Effects, 2, "Eff1,Eff2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularBindingEnergies, ScopingType.MolecularDockingModels, 5, "MD1,MD2,MD3", "MD4,MD5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularBindingEnergies, ScopingType.Compounds, 5, "a,b,c,d", "e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1,Eff2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,d");
        }

        [TestMethod]
        public void DataLinkingMolecularBindingEnergiesFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.MolecularDockingModels, @"MolecularDockingModelsTests/MolecularDockingModelsSimple"),
                (ScopingType.MolecularBindingEnergies, @"MolecularDockingModelsTests/MolecularBindingEnergiesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B", "D"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.MolecularDockingModels);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.MolecularDockingModels);
            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.MolecularDockingModels);
            AssertDataReadingSummaryRecord(report, ScopingType.MolecularDockingModels, 3, "MD1,MD3", "MD2", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularDockingModels, ScopingType.Effects, 2, "Eff1", "Eff2", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularBindingEnergies, ScopingType.MolecularDockingModels, 5, "MD1,MD3", "MD2,MD4,MD5", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.MolecularBindingEnergies, ScopingType.Compounds, 5, "a,b,d", "c,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "Eff1");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,d");

        }
    }
}
