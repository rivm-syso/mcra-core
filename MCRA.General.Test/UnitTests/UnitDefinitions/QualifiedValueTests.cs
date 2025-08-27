namespace MCRA.General.Test.UnitTests.UnitDefinitions {
    [TestClass]
    public class QualifiedValueTests {

        [TestMethod]
        public void QualifiedValue_TestEquals() {
            QualifiedValue value, other;

            value = new QualifiedValue(25, ValueQualifier.Equals);
            other = new QualifiedValue(25, ValueQualifier.Equals);
            Assert.IsTrue(value == other);

            value = new QualifiedValue(25, ValueQualifier.LessThan);
            other = new QualifiedValue(25, ValueQualifier.LessThan);
            Assert.IsTrue(value == other);

            value = new QualifiedValue(25, ValueQualifier.Equals);
            other = new QualifiedValue(25, ValueQualifier.LessThan);
            Assert.IsFalse(value == other);

            value = new QualifiedValue(25, ValueQualifier.Equals);
            other = new QualifiedValue(25.1, ValueQualifier.Equals);
            Assert.IsFalse(value == other);
        }

        [TestMethod]
        public void QualifiedValue_TestCompareTo() {
            QualifiedValue val1, val2;

            val1 = new QualifiedValue(25, ValueQualifier.Equals);
            val2 = new QualifiedValue(25, ValueQualifier.Equals);
            Assert.IsFalse(val1 > val2);
            Assert.IsFalse(val1 < val2);
            Assert.IsFalse(val2 > val1);
            Assert.IsFalse(val2 < val1);

            val1 = new QualifiedValue(25, ValueQualifier.LessThan);
            val2 = new QualifiedValue(25, ValueQualifier.LessThan);
            Assert.IsFalse(val1 > val2);
            Assert.IsFalse(val1 < val2);
            Assert.IsFalse(val2 > val1);
            Assert.IsFalse(val2 < val1);

            val1 = new QualifiedValue(25, ValueQualifier.Equals);
            val2 = new QualifiedValue(25, ValueQualifier.LessThan);
            Assert.IsTrue(val1 > val2);
            Assert.IsFalse(val1 < val2);
            Assert.IsFalse(val2 > val1);
            Assert.IsTrue(val2 < val1);

            val1 = new QualifiedValue(25, ValueQualifier.Equals);
            val2 = new QualifiedValue(25.1, ValueQualifier.Equals);
            Assert.IsTrue(val1 < val2);
            Assert.IsFalse(val1 > val2);
            Assert.IsTrue(val2 > val1);
            Assert.IsFalse(val2 < val1);

            val1 = new QualifiedValue(25, ValueQualifier.Equals);
            val2 = new QualifiedValue(25.1, ValueQualifier.LessThan);
            Assert.IsFalse(val1 < val2);
            Assert.IsFalse(val1 > val2);
            Assert.IsFalse(val2 > val1);
            Assert.IsFalse(val2 < val1);
        }
    }
}
