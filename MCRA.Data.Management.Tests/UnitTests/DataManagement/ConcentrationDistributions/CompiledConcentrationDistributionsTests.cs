using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledConcentrationDistributionsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationDistributions_SimpleTest(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            var distributions = GetAllConcentrationDistributions(managerType);

            Assert.HasCount(15, distributions);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "t3", "f4", "f5", "t5", "f7", "f8" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, codes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationDistributions_SimpleFoodsFilterTest(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);

            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            //set a filter scope on foods
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "t3"]);

            var distributions = GetAllConcentrationDistributions(managerType);

            Assert.HasCount(4, distributions);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "t3" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "C", "D", "F" }, codes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationDistributions_SimpleCompoundsFilterTest(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);

            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            //set a filter scope on compounds
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);
            var distributions = GetAllConcentrationDistributions(managerType);

            Assert.HasCount(6, distributions);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f2", "t3", "f4", "f7" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, codes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledConcentrationDistributions_FilterFoodsAndCompoundsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);

            RawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "t3"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);
            var distributions = GetAllConcentrationDistributions(managerType);

            Assert.HasCount(1, distributions);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "t3" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "C" }, codes);
        }
    }
}
