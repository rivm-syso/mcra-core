using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitDefinitions {
    [TestClass]
    public class QualifiedValueExtensionsTests {

        [TestMethod]
        public void QualifiedValueExtensions_TestAverage1() {
            var values = new QualifiedValue[] {
                new(25, ValueQualifier.Equals),
                new(30, ValueQualifier.Equals),
                new(20, ValueQualifier.Equals),
            };
            var result = values.Average();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.AreEqual(25, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestAverage2() {
            var values = new QualifiedValue[] {
                new(25, ValueQualifier.LessThan),
                new(25, ValueQualifier.LessThan),
                new(25, ValueQualifier.LessThan),
            };
            var result = values.Average();
            Assert.AreEqual(ValueQualifier.LessThan, result.Qualifier);
            Assert.AreEqual(25, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestAverage3() {
            var values = new QualifiedValue[] {
                new(25, ValueQualifier.LessThan)
            };
            var result = values.Average();
            Assert.AreEqual(ValueQualifier.LessThan, result.Qualifier);
            Assert.AreEqual(25, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestAverage4() {
            var values = new QualifiedValue[] {
                new(25, ValueQualifier.Equals),
                new(25, ValueQualifier.LessThan)
            };
            var result = values.Average();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.IsTrue(double.IsNaN(result.Value));
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestAverage5() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.LessThan),
                new(30, ValueQualifier.LessThan)
            };
            var result = values.Average();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.IsTrue(double.IsNaN(result.Value));
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestAverage6() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.LessThan),
                new(30, ValueQualifier.LessThan),
            };
            var result = values.Average();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.IsTrue(double.IsNaN(result.Value));
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestAverageEmpty() {
            var values = new QualifiedValue[0];
            var result = values.Average();
            Assert.IsTrue(result.IsNan());
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMax1() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.LessThan),
                new(30, ValueQualifier.LessThan),
            };
            var result = values.Max();
            Assert.AreEqual(ValueQualifier.LessThan, result.Qualifier);
            Assert.AreEqual(30, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMax2() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.Equals),
                new(30, ValueQualifier.Equals),
            };
            var result = values.Max();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.AreEqual(30, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMax3() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.Equals),
                new(30, ValueQualifier.LessThan),
            };
            var result = values.Max();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.IsTrue(double.IsNaN(result.Value));
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMax4() {
            var values = new QualifiedValue[] {
                new(30, ValueQualifier.Equals),
                new(20, ValueQualifier.LessThan),
            };
            var result = values.Max();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.AreEqual(30, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMax5() {
            var values = new QualifiedValue[] {
                new(30, ValueQualifier.Equals),
                new(30, ValueQualifier.LessThan),
            };
            var result = values.Max();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.AreEqual(30, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMaxEmpty() {
            var values = new QualifiedValue[0];
            var result = values.Max();
            Assert.IsTrue(result.IsNan());
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMin1() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.LessThan),
                new(30, ValueQualifier.LessThan),
            };
            var result = values.Min();
            Assert.AreEqual(ValueQualifier.LessThan, result.Qualifier);
            Assert.AreEqual(20, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMin2() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.Equals),
                new(30, ValueQualifier.Equals),
            };
            var result = values.Min();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.AreEqual(20, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMin3() {
            var values = new QualifiedValue[] {
                new(20, ValueQualifier.Equals),
                new(30, ValueQualifier.LessThan),
            };
            var result = values.Min();
            Assert.AreEqual(ValueQualifier.Equals, result.Qualifier);
            Assert.IsTrue(double.IsNaN(result.Value));
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMin4() {
            var values = new QualifiedValue[] {
                new(30, ValueQualifier.Equals),
                new(20, ValueQualifier.LessThan),
            };
            var result = values.Min();
            Assert.AreEqual(ValueQualifier.LessThan, result.Qualifier);
            Assert.AreEqual(20, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMin5() {
            var values = new QualifiedValue[] {
                new(30, ValueQualifier.Equals),
                new(30, ValueQualifier.LessThan),
            };
            var result = values.Min();
            Assert.AreEqual(ValueQualifier.LessThan, result.Qualifier);
            Assert.AreEqual(30, result.Value);
        }

        [TestMethod]
        public void QualifiedValueExtensions_TestMinEmpty() {
            var values = new QualifiedValue[0];
            var result = values.Min();
            Assert.IsTrue(result.IsNan());
        }
    }
}
