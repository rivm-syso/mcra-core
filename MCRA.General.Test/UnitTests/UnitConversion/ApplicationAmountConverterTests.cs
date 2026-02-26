namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ApplicationAmountConverterTests {
        [TestMethod]
        public void ApplicationAmountConverterTests_Test1() {
            Assert.AreEqual(1e3, ApplicationAmountUnit.kg.GetMultiplicationFactor(ConcentrationMassUnit.Grams));
            Assert.AreEqual(1e6, ApplicationAmountUnit.kg.GetMultiplicationFactor(ConcentrationMassUnit.MilliGrams));
            Assert.AreEqual(1e-3, ApplicationAmountUnit.g.GetMultiplicationFactor(ConcentrationMassUnit.Kilograms));
            Assert.AreEqual(1e-6, ApplicationAmountUnit.mg.GetMultiplicationFactor(ConcentrationMassUnit.Kilograms));
            Assert.AreEqual(1e3, ApplicationAmountUnit.g.GetMultiplicationFactor(ConcentrationMassUnit.MilliGrams));
            Assert.AreEqual(1e-3, ApplicationAmountUnit.mg.GetMultiplicationFactor(ConcentrationMassUnit.Grams));
            Assert.AreEqual(1e-6, ApplicationAmountUnit.mg.GetMultiplicationFactor(ConcentrationMassUnit.Kilograms));
            Assert.AreEqual(1, ApplicationAmountUnit.kg.GetMultiplicationFactor(ConcentrationMassUnit.Kilograms));
            Assert.AreEqual(1, ApplicationAmountUnit.g.GetMultiplicationFactor(ConcentrationMassUnit.Grams));
            Assert.AreEqual(1, ApplicationAmountUnit.mg.GetMultiplicationFactor(ConcentrationMassUnit.MilliGrams));

        }


        [TestMethod]
        public void ApplicationAmountConverterTests_TestGetMultiplicationFactorAll() {
            var enumValues = Enum.GetValues(typeof(ConcentrationMassUnit))
                .Cast<ConcentrationMassUnit>()
                .Where(r => r != ConcentrationMassUnit.Undefined)
                .ToList();
            foreach (var unit in enumValues) {
                foreach (var target in enumValues) {
                    var value = unit.GetMultiplicationFactor(target, 70);
                    Assert.IsFalse(double.IsNaN(value));
                }
            }
        }
    }
}
