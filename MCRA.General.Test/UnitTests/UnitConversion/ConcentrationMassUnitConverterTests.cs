using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ConcentrationMassUnitConverterTests {
        [TestMethod]
        public void ConcentrationMassUnitConverter_Test1() {
            Assert.AreEqual(1e3, ConcentrationMassUnit.Kilograms.GetMultiplicationFactor(ConcentrationMassUnit.Grams));
            Assert.AreEqual(1e-3, ConcentrationMassUnit.Grams.GetMultiplicationFactor(ConcentrationMassUnit.Kilograms));
            Assert.AreEqual(1D / 70D, ConcentrationMassUnit.Kilograms.GetMultiplicationFactor(ConcentrationMassUnit.PerUnit, 70));
            Assert.AreEqual(70, ConcentrationMassUnit.PerUnit.GetMultiplicationFactor(ConcentrationMassUnit.Kilograms, 70));
            Assert.AreEqual(1D / 70D, ConcentrationMassUnit.Grams.GetMultiplicationFactor(ConcentrationMassUnit.PerUnit, 70));
            Assert.AreEqual(70, ConcentrationMassUnit.PerUnit.GetMultiplicationFactor(ConcentrationMassUnit.Grams, 70));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ConcentrationMassUnitConverter_Test2() {
            Assert.AreEqual(1e3, ConcentrationMassUnit.Grams.GetMultiplicationFactor(ConcentrationMassUnit.PerUnit));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ConcentrationMassUnitConverter_Test3() {
            Assert.AreEqual(1e3, ConcentrationMassUnit.PerUnit.GetMultiplicationFactor(ConcentrationMassUnit.Grams));
        }

        [TestMethod]
        public void ConcentrationMassUnitConverter_TestGetMultiplicationFactorAll() {
            var enumValues = Enum.GetValues(typeof(ConcentrationMassUnit))
                .Cast<ConcentrationMassUnit>()
                .Where(r => r != ConcentrationMassUnit.Undefined)
                .ToList();
            foreach (var unit in enumValues) {
                foreach (var target in enumValues) {
                    var value = unit.GetMultiplicationFactor(target, 70);
                    Assert.IsTrue(!double.IsNaN(value));
                }
            }
        }
    }
}
