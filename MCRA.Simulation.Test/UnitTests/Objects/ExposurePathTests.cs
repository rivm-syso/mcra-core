using MCRA.General;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Test.UnitTests.Objects {
    [TestClass]
    public class ExposurePathTests {

        [TestMethod]
        public void ExposurePath_TestEquals_Equal1() {
            var p1 = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral);
            var p2 = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral);
            Assert.IsTrue(p1 == p2);
            Assert.AreEqual(p1, p2);
        }

        [TestMethod]
        public void ExposurePath_Equality_DifferentValues_ShouldNotBeEqual() {
            var p1 = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral);
            var p2 = new ExposurePath(ExposureSource.Soil, ExposureRoute.Inhalation);

            Assert.AreNotEqual(p1, p2);
            Assert.IsFalse(p1.Equals(p2));
        }

        [TestMethod]
        public void ExposurePath_GetHashCode_SameValues_ShouldBeSame() {
            var p1 = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral);
            var p2 = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral);

            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
        }

        [TestMethod]
        public void ExposurePath_GetHashCode_DifferentValues_ShouldBeDifferent() {
            var p1 = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral);
            var p2 = new ExposurePath(ExposureSource.Soil, ExposureRoute.Dermal);

            Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
        }

        [TestMethod]
        public void ExposurePath_Dictionary_StoresUniqueKeys() {
            var p1 = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral);
            var p2 = new ExposurePath(ExposureSource.Soil, ExposureRoute.Dermal);
            var p1_clone = new ExposurePath(ExposureSource.Dust, ExposureRoute.Oral); // Same as p1

            var p1_value = "Exposure path 1";
            var p2_value = "Exposure path 2";
            var p4_value = "Exposure path 4";
            var pathDictionary = new Dictionary<ExposurePath, string>
            {                   
                { p1, p1_value },
                { p2, p2_value }
            };

            // Cannot add exposure path with the same key
            Assert.ThrowsExactly<ArgumentException>(() => pathDictionary.Add(p1_clone, "Exposure path 1 clone"));

            // Check that p1 and p1_clone behave as the same key
            Assert.IsTrue(pathDictionary.ContainsKey(p1_clone));
            Assert.AreEqual(pathDictionary[p1_clone], p1_value);

            // Add a unique key and verify
            var p4 = new ExposurePath(ExposureSource.OtherNonDiet, ExposureRoute.Inhalation);
            pathDictionary[p4] = p4_value;

            Assert.IsTrue(pathDictionary.ContainsKey(p4));
            Assert.AreEqual(pathDictionary[p4], p4_value);

            // Dictionary should contain exactly 3 unique keys (p1 and p1_clone are the same)
            Assert.HasCount(3, pathDictionary);            
        }
    }
}
