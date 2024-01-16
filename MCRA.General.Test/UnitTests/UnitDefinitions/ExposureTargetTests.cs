using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitDefinitions {
    [TestClass]
    public class ExposureTargetTests {

        [TestMethod]
        public void ExposureTarget_TestEquals_Equal1() {
            var t1 = new ExposureTarget(ExposurePathType.Dietary);
            var t2 = new ExposureTarget(ExposurePathType.Dietary);
            Assert.IsTrue(t1 == t2);
            Assert.AreEqual(t1, t2);
        }

        [TestMethod]
        public void ExposureTarget_TestEquals_Equal2() {
            var t1 = new ExposureTarget(BiologicalMatrix.Urine, ExpressionType.Creatinine);
            var t2 = new ExposureTarget(BiologicalMatrix.Urine, ExpressionType.Creatinine);
            Assert.IsTrue(t1 == t2);
            Assert.AreEqual(t1, t2);
        }

        [TestMethod]
        public void ExposureTarget_TestEquals_EqualNull() {
            ExposureTarget t1 = null;
            ExposureTarget t2 = null;
            Assert.IsTrue(t1 == t2);
            Assert.AreEqual(t1, t2);
        }

        [TestMethod]
        public void ExposureTarget_TestEquals_NotEqual1() {
            var t1 = new ExposureTarget(ExposurePathType.Dietary);
            var t2 = new ExposureTarget(ExposurePathType.Oral);
            Assert.IsTrue(t1 != t2);
            Assert.AreNotEqual(t1, t2);
        }

        [TestMethod]
        public void ExposureTarget_TestEquals_NotEqual2() {
            var t1 = new ExposureTarget(BiologicalMatrix.Urine, ExpressionType.Creatinine);
            var t2 = new ExposureTarget(BiologicalMatrix.Urine, ExpressionType.None);
            Assert.IsTrue(t1 != t2);
            Assert.AreNotEqual(t1, t2);
        }

        [TestMethod]
        public void ExposureTarget_TestEquals_NotEqual_Null() {
            var t1 = new ExposureTarget(BiologicalMatrix.Urine, ExpressionType.Creatinine);
            ExposureTarget t2 = null;
            Assert.IsTrue(t1 != t2);
            Assert.AreNotEqual(t1, t2);
        }
    }
}
