using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock individual properties
    /// </summary>
    public static class MockIndividualPropertiesGenerator {

        /// <summary>
        /// Fake age property.
        /// </summary>
        public static IndividualProperty FakeAgeProperty = new IndividualProperty() {
            Code = "Age",
            PropertyType = IndividualPropertyType.Nonnegative,
            Min = 0,
            Max = 100,
        };

        /// <summary>
        /// Fake gender property.
        /// </summary>
        public static IndividualProperty FakeGenderProperty = new IndividualProperty() {
            Code = "Gender",
            PropertyType = IndividualPropertyType.Gender,
            CategoricalLevels = new HashSet<string>() { "male", "female" }
        };

        /// <summary>
        /// Fake age property.
        /// </summary>
        public static IndividualProperty FakeBooleanProperty = new IndividualProperty() {
            Code = "Bool",
            PropertyType = IndividualPropertyType.Boolean
        };

        /// <summary>
        /// Creates a fake individual property of the specified type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="propertyType"></param>
        /// <param name="levels"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static IndividualProperty CreateFake(
            string name,
            IndividualPropertyType propertyType,
            HashSet<string> levels = null,
            double? min = null,
            double? max = null
        ) {
            return new IndividualProperty() {
                Code = name,
                PropertyType = propertyType,
                CategoricalLevels = levels,
                Min = min ?? double.NaN,
                Max = max ?? double.NaN
            };
        }

        /// <summary>
        /// Creates a list of individual properties.
        /// </summary>
        /// <returns></returns>
        public static List<IndividualProperty> Create() {
            return new List<IndividualProperty>() {
                FakeGenderProperty,
                FakeAgeProperty
            };
        }
    }
}
