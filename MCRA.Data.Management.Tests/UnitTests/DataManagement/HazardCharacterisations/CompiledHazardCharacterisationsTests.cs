using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledHazardCharacterisationsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHazardCharacterisationsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );

            var records = GetAllHazardCharacterisations(managerType);
            var doseUnits = records.Select(r => r.DoseUnit).ToList();
            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4" }, effectCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHazardCharacterisationsSimpleEffectsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var records = GetAllHazardCharacterisations(managerType);
            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHazardCharacterisationsSimpleCompoundsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var records = GetAllHazardCharacterisations(managerType);

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3" }, effectCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHazardCharacterisationsFilterEffectsAndCompoundsSimpleTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests/HazardCharacterisationsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff4"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var records = GetAllHazardCharacterisations(managerType);

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }
    }
}
