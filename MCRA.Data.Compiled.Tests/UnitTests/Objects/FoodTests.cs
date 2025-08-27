using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Test {
    [TestClass]
    public class FoodTests {

        #region Mocks

        private FoodUnitWeight mockFoodUnitWeight(
            string location,
            UnitWeightValueType valueType,
            ValueQualifier qualifier,
            double value
        ) {
            return new FoodUnitWeight() {
                Location = location,
                Qualifier = qualifier,
                ValueType = valueType,
                Value = value
            };
        }

        #endregion

        [TestMethod]
        public void Food_TestGetUnitWeightsEp1() {
            var food = new Food() {
                DefaultUnitWeightEp = mockFoodUnitWeight(null, UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 22.5),
                FoodUnitWeights = [
                    mockFoodUnitWeight("NL", UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 30),
                    mockFoodUnitWeight("DE", UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 20),
                ]
            };
            Assert.AreEqual(ValueQualifier.Equals, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp).Qualifier);
            Assert.AreEqual(22.5, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp).Value);
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "NL").Qualifier);
            Assert.AreEqual(30, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "NL").Value);
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Qualifier);
            Assert.AreEqual(25, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Value);
        }

        [TestMethod]
        public void Food_TestGetUnitWeightsEp2() {
            var food = new Food() {
                FoodUnitWeights = [
                    mockFoodUnitWeight("NL", UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 30),
                    mockFoodUnitWeight("DE", UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 20),
                ]
            };
            Assert.IsNull(food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp));
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "NL").Qualifier);
            Assert.AreEqual(30, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "NL").Value);
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Qualifier);
            Assert.AreEqual(25, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Value);
        }

        [TestMethod]
        public void Food_TestGetUnitWeightsEp3() {
            var food = new Food() {
                DefaultUnitWeightEp = mockFoodUnitWeight(null, UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 22.5)
            };
            Assert.AreEqual(ValueQualifier.Equals, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp).Qualifier);
            Assert.AreEqual(22.5, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp).Value);

            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Qualifier);
            Assert.AreEqual(22.5, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Value);
        }

        [TestMethod]
        public void Food_TestGetUnitWeightsEp4() {
            var food = new Food() {
                DefaultUnitWeightEp = mockFoodUnitWeight(null, UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 22.5),
                FoodUnitWeights = [
                    mockFoodUnitWeight("NL", UnitWeightValueType.UnitWeightEp, ValueQualifier.LessThan, 30),
                    mockFoodUnitWeight("DE", UnitWeightValueType.UnitWeightEp, ValueQualifier.Equals, 20),
                ]
            };
            Assert.AreEqual(ValueQualifier.Equals, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp).Qualifier);
            Assert.AreEqual(22.5, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightEp).Value);

            Assert.AreEqual(ValueQualifier.LessThan, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "NL").Qualifier);
            Assert.AreEqual(30, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "NL").Value);
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Qualifier);
            Assert.AreEqual(22.5, food.GetUnitWeight(UnitWeightValueType.UnitWeightEp, "FR").Value);
        }

        [TestMethod]
        public void Food_TestGetUnitWeightsRac1() {
            var food = new Food() {
                DefaultUnitWeightRac = mockFoodUnitWeight(null, UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 22.5),
                FoodUnitWeights = [
                    mockFoodUnitWeight("NL", UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 30),
                    mockFoodUnitWeight("DE", UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 20),
                ]
            };
            Assert.AreEqual(ValueQualifier.Equals, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac).Qualifier);
            Assert.AreEqual(22.5, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac).Value);

            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "NL").Qualifier);
            Assert.AreEqual(30, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "NL").Value);
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Qualifier);
            Assert.AreEqual(25, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Value);
        }

        [TestMethod]
        public void Food_TestGetUnitWeightsRac2() {
            var food = new Food() {
                FoodUnitWeights = [
                    mockFoodUnitWeight("NL", UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 30),
                    mockFoodUnitWeight("DE", UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 20),
                ]
            };
            Assert.IsNull(food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac));
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "NL").Qualifier);

            Assert.AreEqual(30, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "NL").Value);
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Qualifier);
            Assert.AreEqual(25, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Value);
        }

        [TestMethod]
        public void Food_TestGetUnitWeightsRac3() {
            var food = new Food() {
                DefaultUnitWeightRac = mockFoodUnitWeight(null, UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 22.5)
            };
            Assert.AreEqual(ValueQualifier.Equals, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac).Qualifier);
            Assert.AreEqual(22.5, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac).Value);

            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Qualifier);
            Assert.AreEqual(22.5, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Value);
        }

        [TestMethod]
        public void Food_TestGetUnitWeightsRac4() {
            var food = new Food() {
                DefaultUnitWeightRac = mockFoodUnitWeight(null, UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 22.5),
                FoodUnitWeights = [
                    mockFoodUnitWeight("NL", UnitWeightValueType.UnitWeightRac, ValueQualifier.LessThan, 30),
                    mockFoodUnitWeight("DE", UnitWeightValueType.UnitWeightRac, ValueQualifier.Equals, 20),
                ]
            };
            Assert.AreEqual(ValueQualifier.Equals, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac).Qualifier);
            Assert.AreEqual(22.5, food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac).Value);

            Assert.AreEqual(ValueQualifier.LessThan, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "NL").Qualifier);
            Assert.AreEqual(30, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "NL").Value);
            Assert.AreEqual(ValueQualifier.Equals, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Qualifier);
            Assert.AreEqual(22.5, food.GetUnitWeight(UnitWeightValueType.UnitWeightRac, "FR").Value);
        }
    }
}
