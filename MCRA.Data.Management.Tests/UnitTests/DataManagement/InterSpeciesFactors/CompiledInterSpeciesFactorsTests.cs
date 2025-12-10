using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledInterSpeciesFactorsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledInterSpeciesFactorsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests/InterSpeciesModelParametersSimple")
            );

            var factors = GetAllInterSpeciesFactors(managerType);

            Assert.AreEqual(2, factors.Count(f => f.Compound == null));
            Assert.AreEqual(9, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5", "Eff6" }, effectCodes.ToList());
        }


        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledInterSpeciesFactorsSimpleEffectsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests/InterSpeciesModelParametersSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var factors = GetAllInterSpeciesFactors(managerType);
            Assert.HasCount(1, factors);
            var f = factors.First();

            Assert.IsNull(f.Compound);
            Assert.AreEqual("Eff1", f.Effect.Code);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledInterSpeciesFactorsSimpleCompoundsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests/InterSpeciesModelParametersSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllInterSpeciesFactors(managerType);

            Assert.AreEqual(2, factors.Count(f => f.Compound == null));
            Assert.AreEqual(4, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effectCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledInterSpeciesModelParametersFilterEffectsAndCompoundsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests/InterSpeciesModelParametersSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff4"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllInterSpeciesFactors(managerType);

            Assert.AreEqual(1, factors.Count(f => f.Compound == null));
            Assert.AreEqual(1, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff4" }, effectCodes.ToList());
        }
    }
}
