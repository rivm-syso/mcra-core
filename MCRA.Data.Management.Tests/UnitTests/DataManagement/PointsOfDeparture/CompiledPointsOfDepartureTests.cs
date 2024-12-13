using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledPointsOfDepartureTests : CompiledTestsBase {
        protected Func<ICollection<Compiled.Objects.PointOfDeparture>> _getItemsDelegate;

        [TestMethod]
        public void CompiledPointsOfDeparture_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests\HazardDosesSimple")
            );

            var factors = _getItemsDelegate.Invoke();

            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4" }, effectCodes.ToList());
        }


        [TestMethod]
        public void CompiledPointsOfDeparture_TestSimpleEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests\HazardDosesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var factors = _getItemsDelegate.Invoke();
            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }

        [TestMethod]
        public void CompiledPointsOfDeparture_TestSimpleCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests\HazardDosesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = _getItemsDelegate.Invoke();

            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3" }, effectCodes.ToList());
        }

        [TestMethod]
        public void CompiledPointsOfDeparture_TestFilterEffectsAndCompoundsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.PointsOfDeparture, @"HazardDosesTests\HazardDosesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff4"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = _getItemsDelegate.Invoke();

            var compoundCodes = factors.Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }
    }
}
