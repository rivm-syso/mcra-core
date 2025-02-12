using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledConcentrationDistributionsTests : CompiledTestsBase {

        protected Func<ICollection<ConcentrationDistribution>> _getConcentrationDistributionsDelegate;
        protected bool _isSubSetManagerTest = false;

        [TestMethod]
        public void CompiledConcentrationDistributions_SimpleTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            var distributions = _getConcentrationDistributionsDelegate.Invoke();

            Assert.AreEqual(15, distributions.Count);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "t3", "f4", "f5", "t5", "f7", "f8" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, codes);
        }

        [TestMethod]
        public void CompiledConcentrationDistributions_SimpleFoodsFilterTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);

            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            //set a filter scope on foods
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "t3"]);

            var distributions = _getConcentrationDistributionsDelegate.Invoke();

            Assert.AreEqual(4, distributions.Count);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f1", "t3" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "C", "D", "F" }, codes);
        }

        [TestMethod]
        public void CompiledConcentrationDistributions_SimpleCompoundsFilterTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);

            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            //set a filter scope on compounds
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);
            var distributions = _getConcentrationDistributionsDelegate.Invoke();

            Assert.AreEqual(6, distributions.Count);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "f2", "t3", "f4", "f7" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, codes);
        }

        [TestMethod]
        public void CompiledConcentrationDistributions_FilterFoodsAndCompoundsSimpleTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);

            _rawDataProvider.SetDataTables(
                (ScopingType.ConcentrationDistributions, @"ConcentrationsTests/ConcentrationDistributionsSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "t3"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);
            var distributions = _getConcentrationDistributionsDelegate.Invoke();

            Assert.AreEqual(1, distributions.Count);

            var codes = distributions.Select(c => c.Food.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "t3" }, codes);

            codes = distributions.Select(c => c.Compound.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "C" }, codes);
        }
    }
}
