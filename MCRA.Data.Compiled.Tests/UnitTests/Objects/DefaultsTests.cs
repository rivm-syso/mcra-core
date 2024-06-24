using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Compiled.Test.UnitTests.DefaultTests {
    /// <summary>
    /// Summary description for DefaultsTests
    /// </summary>
    [TestClass]
    public class DefaultsTests {

        [TestMethod]
        public void Defaults_TestCompoundDefaultConcentrationUnit() {
            var cmp = new Compound();
            Assert.AreEqual(ConcentrationUnit.mgPerKg, cmp.ConcentrationUnit);
        }

        [TestMethod]
        public void Defaults_TestAnalyticalMethodCompoundDefaultConcentrationUnit() {
            var amc = new AnalyticalMethodCompound();
            Assert.AreEqual(ConcentrationUnit.mgPerKg, amc.ConcentrationUnit);
        }

        [TestMethod]
        public void Defaults_TestDoseResponseModelDoseUnit() {
            var drm = new Objects.PointOfDeparture();
            Assert.AreEqual(DoseUnit.mgPerKgBWPerDay, drm.DoseUnit);
        }

        [TestMethod]
        public void Defaults_TestFoodSurveyBodyWeightUnit() {
            var r = new FoodSurvey();
            Assert.AreEqual(BodyWeightUnit.kg, r.BodyWeightUnit);
        }

        [TestMethod]
        public void Defaults_TestFoodSurveyConsumptionUnit() {
            var r = new FoodSurvey();
            Assert.AreEqual(ConsumptionUnit.g, r.ConsumptionUnit);
        }
    }
}
