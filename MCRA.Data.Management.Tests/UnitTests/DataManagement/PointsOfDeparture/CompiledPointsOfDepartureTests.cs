using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledPointsOfDepartureTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledPointsOfDeparture_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );

            var factors = GetAllPointsOfDeparture(managerType);

            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4" }, effectCodes.ToList());
        }


        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledPointsOfDeparture_TestSimpleEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var factors = GetAllPointsOfDeparture(managerType);
            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledPointsOfDeparture_TestSimpleCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllPointsOfDeparture(managerType);

            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3" }, effectCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledPointsOfDeparture_TestFilterEffectsAndCompoundsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests/HazardDosesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff4"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllPointsOfDeparture(managerType);

            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }
    }
}
