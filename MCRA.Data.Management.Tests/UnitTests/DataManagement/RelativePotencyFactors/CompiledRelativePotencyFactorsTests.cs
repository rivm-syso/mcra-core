using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledRelativePotencyFactorsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledRelativePotencyFactors_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );

            var factors = GetAllRelativePotencyFactors(managerType);

            var compoundCodes = factors.SelectMany(f => f.Value).Select(f => f.Compound.Code).Distinct().ToList();
            var effectCodes = factors.SelectMany(f => f.Value).Select(f => f.Effect.Code).Distinct().ToList();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5", "Eff6" }, effectCodes);
        }


        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledRelativePotencyFactors_TestEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var factors = GetAllRelativePotencyFactors(managerType);
            Assert.HasCount(1, factors);
            var f = factors.Values.Single().Single();

            Assert.AreEqual("B", f.Compound.Code);
            Assert.AreEqual("Eff1", f.Effect.Code);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledRelativePotencyFactors_TestCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllRelativePotencyFactors(managerType);

            var compoundCodes = factors.SelectMany(f => f.Value).Select(f => f.Compound.Code).Distinct().ToList();
            var effectCodes = factors.SelectMany(f => f.Value).Select(f => f.Effect.Code).Distinct().ToList();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effectCodes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledRelativePotencyFactors_TestFilterEffectsAndCompoundsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.RelativePotencyFactors, @"RelativePotencyFactorsTests/RelativePotencyFactorsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff4"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllRelativePotencyFactors(managerType);

            var compoundCodes = factors.SelectMany(f => f.Value).Select(f => f.Compound.Code).Distinct().ToList();
            var effectCodes = factors.SelectMany(f => f.Value).Select(f => f.Effect.Code).Distinct().ToList();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff4" }, effectCodes);
        }
    }
}
