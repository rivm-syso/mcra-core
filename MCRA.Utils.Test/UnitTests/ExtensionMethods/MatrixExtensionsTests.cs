using System.Diagnostics;
using System.Linq;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class MatrixExtensionMethodsTests {

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestEmptySameDimensions() {
            var matrix1 = new double[3, 4];
            var matrix2 = new double[3, 4];
            Assert.IsTrue(matrix1.MatrixEquals(matrix2));
        }

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestEmptyDifferentDimensions() {
            var matrix1 = new double[2, 4];
            var matrix2 = new double[4, 2];
            Assert.IsFalse(matrix1.MatrixEquals(matrix2));
        }

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestEmptyDifferentRows() {
            var matrix1 = new double[2, 4];
            var matrix2 = new double[3, 4];
            Assert.IsFalse(matrix1.MatrixEquals(matrix2));
        }

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestEmptyDifferentColumns() {
            var matrix1 = new double[2, 3];
            var matrix2 = new double[2, 4];
            Assert.IsFalse(matrix1.MatrixEquals(matrix2));
        }

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestEqualColumnVector() {
            var matrix1 = new int[3, 1] {
                {0},
                {4},
                {8}
            };
            var matrix2 = new int[3, 1] {
                {0},
                {4},
                {8}
            };
            Assert.IsTrue(matrix1.MatrixEquals(matrix2));
        }

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestUnequalColumnVector() {
            var matrix1 = new int[3, 1] {
                {0},
                {4},
                {8}
            };
            var matrix2 = new int[3, 1] {
                {1},
                {1},
                {1}
            };
            Assert.IsFalse(matrix1.MatrixEquals(matrix2));
        }

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestEqualMatrices() {
            var matrix1 = new int[3, 4] {
                {0, 1, 2, 3},
                {4, 5, 6, 7},
                {8, 9, 10, 11}
            };
            var matrix2 = new int[3, 4] {
                {0, 1, 2, 3},
                {4, 5, 6, 7},
                {8, 9, 10, 11}
            };
            Assert.IsTrue(matrix1.MatrixEquals(matrix2));
        }

        [TestMethod]
        public void MatrixExtensionMethodsTests_TestUnequalMatrices() {
            var matrix1 = new int[3, 4] {
                {0, 1, 2, 3},
                {4, 5, 6, 7},
                {8, 9, 10, 11}
            };
            var matrix2 = new int[3, 4] {
                {1, 1, 1, 1},
                {1, 1, 1, 1},
                {1, 1, 1, 1}
            };
            Assert.IsFalse(matrix1.MatrixEquals(matrix2));
        }
    }
}
