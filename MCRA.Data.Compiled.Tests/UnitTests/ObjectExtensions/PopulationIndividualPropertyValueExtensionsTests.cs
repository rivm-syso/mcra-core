using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Test {
    [TestClass]
    public class PopulationIndividualPropertyValueExtensionsTests {

        [TestMethod]
        public void PopulationIndividualPropertyValueExtensions_TestEmpty() {
            var value = new PopulationIndividualPropertyValue();
            var months = value.GetMonths();
            Assert.IsNull(months);
        }

        [TestMethod]
        public void PopulationIndividualPropertyValueExtensions_TestSingleValue() {
            var value = new PopulationIndividualPropertyValue() {
                Value = "January"
            };
            var months = value.GetMonths();
            CollectionAssert.AreEquivalent(new[] { 1 }, months);
        }

        [TestMethod]
        public void PopulationIndividualPropertyValueExtensions_TestFail() {
            var value = new PopulationIndividualPropertyValue() {
                Value = "XXX"
            };
            Assert.ThrowsException<Exception>(() => value.GetMonths());
        }

        [TestMethod]
        public void PopulationIndividualPropertyValueExtensions_TestMultipleValues() {
            var value = new PopulationIndividualPropertyValue() {
                Value = "March, May, September"
            };
            var months = value.GetMonths();
            CollectionAssert.AreEquivalent(new[] { 3, 5, 9 }, months);
        }

        [TestMethod]
        public void PopulationIndividualPropertyValueExtensions_TestMinMax() {
            var value = new PopulationIndividualPropertyValue() {
                MinValue = 3,
                MaxValue = 6
            };
            var months = value.GetMonths();
            CollectionAssert.AreEquivalent(new[] { 3, 4, 5, 6 }, months);
        }

        [TestMethod]
        public void PopulationIndividualPropertyValueExtensions_TestMinMaxOverYear() {
            var value = new PopulationIndividualPropertyValue() {
                MinValue = 10,
                MaxValue = 2
            };
            var months = value.GetMonths();
            CollectionAssert.AreEquivalent(new[] { 10, 11, 12, 1, 2 }, months);
        }
    }
}
