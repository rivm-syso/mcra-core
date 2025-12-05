using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledRelativePotencyFactorsTests : CompiledTestsBase {

        protected Func<IDictionary<string, List<RelativePotencyFactor>>> _getItemsDelegate;

        [TestMethod]
        public void CompiledRelativePotencyFactors_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );

            var factors = _getItemsDelegate.Invoke();

            var compoundCodes = factors.SelectMany(f => f.Value).Select(f => f.Compound.Code).Distinct().ToList();
            var effectCodes = factors.SelectMany(f => f.Value).Select(f => f.Effect.Code).Distinct().ToList();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5", "Eff6" }, effectCodes);
        }


        [TestMethod]
        public void CompiledRelativePotencyFactors_TestEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var factors = _getItemsDelegate.Invoke();
            Assert.HasCount(1, factors);
            var f = factors.Values.Single().Single();

            Assert.AreEqual("B", f.Compound.Code);
            Assert.AreEqual("Eff1", f.Effect.Code);
        }

        [TestMethod]
        public void CompiledRelativePotencyFactors_TestCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = _getItemsDelegate.Invoke();

            var compoundCodes = factors.SelectMany(f => f.Value).Select(f => f.Compound.Code).Distinct().ToList();
            var effectCodes = factors.SelectMany(f => f.Value).Select(f => f.Effect.Code).Distinct().ToList();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effectCodes);
        }

        [TestMethod]
        public void CompiledRelativePotencyFactors_TestFilterEffectsAndCompoundsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff4"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = _getItemsDelegate.Invoke();

            var compoundCodes = factors.SelectMany(f => f.Value).Select(f => f.Compound.Code).Distinct().ToList();
            var effectCodes = factors.SelectMany(f => f.Value).Select(f => f.Effect.Code).Distinct().ToList();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff4" }, effectCodes);
        }
    }
}
