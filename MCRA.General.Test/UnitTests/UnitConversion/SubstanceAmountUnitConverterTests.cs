using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class SubstanceAmountUnitConverterTests {
        [TestMethod]
        public void SubstanceAmountUnitConverter_Test1() {
            Assert.AreEqual(1e3, SubstanceAmountUnit.Kilograms.GetMultiplicationFactor(SubstanceAmountUnit.Grams, double.NaN));
            Assert.AreEqual(1 / 50D, SubstanceAmountUnit.Grams.GetMultiplicationFactor(SubstanceAmountUnit.Moles, 50));
            Assert.AreEqual(50, SubstanceAmountUnit.Moles.GetMultiplicationFactor(SubstanceAmountUnit.Grams, 50));
        }

        /// <summary>
        /// Checks whether a unit multiplication factor can be obtained between all pairs
        /// of substance amount units.
        /// </summary>
        [TestMethod]
        public void SubstanceAmountUnitConverter_TestGetMultiplicationFactor() {
            var enumValues = Enum.GetValues(typeof(SubstanceAmountUnit))
                .Cast<SubstanceAmountUnit>()
                .Where(r => r != SubstanceAmountUnit.Undefined)
                .ToList();
            foreach (var unit in enumValues) {
                foreach (var target in enumValues) {
                    var value = unit.GetMultiplicationFactor(target, 100);
                    Assert.IsTrue(!double.IsNaN(value));
                }
            }
        }

        /// <summary>
        /// Tests whether the is-in-moles method is correct for some substance amount units.
        /// </summary>
        [TestMethod]
        public void SubstanceAmountUnitConverter_TestIsInMoles() {
            Assert.IsFalse(SubstanceAmountUnit.Grams.IsInMoles());
            Assert.IsFalse(SubstanceAmountUnit.Femtograms.IsInMoles());
            Assert.IsFalse(SubstanceAmountUnit.Milligrams.IsInMoles());
            Assert.IsFalse(SubstanceAmountUnit.Kilograms.IsInMoles());
            Assert.IsTrue(SubstanceAmountUnit.Moles.IsInMoles());
            Assert.IsTrue(SubstanceAmountUnit.Nanomoles.IsInMoles());
        }
    }
}
