using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledHazardCharacterisationsTests : CompiledTestsBase {
        protected Func<ICollection<HazardCharacterisation>> _getItemsDelegate;

        [TestMethod]
        public void CompiledHazardCharacterisationsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests\HazardCharacterisationsSimple")
            );

            var records = _getItemsDelegate.Invoke();
            var doseUnits = records.Select(r => r.DoseUnit).ToList();
            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4" }, effectCodes.ToList());
        }

        [TestMethod]
        public void CompiledHazardCharacterisationsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests\HazardCharacterisationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var records = _getItemsDelegate.Invoke();
            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }

        [TestMethod]
        public void CompiledHazardCharacterisationsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests\HazardCharacterisationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var records = _getItemsDelegate.Invoke();

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3" }, effectCodes.ToList());
        }

        [TestMethod]
        public void CompiledHazardCharacterisationsFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HazardCharacterisations, @"HazardCharacterisationsTests\HazardCharacterisationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1", "Eff4"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var records = _getItemsDelegate.Invoke();

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var effectCodes = records.Where(r => r.Effect != null).Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effectCodes.ToList());
        }
    }
}
