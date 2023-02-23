using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Numerics {

    /// <summary>
    /// General matrix tests.
    /// </summary>
    [TestClass]
    public class GeneralMatrixTests {

        #region Test matrices

        private static GeneralMatrix _matX {
            get {
                var x = new double[,] {
                    { 1, 2, 3 },
                    { 4, 5, 6 }
                };
                return new GeneralMatrix(x);
            }
        }

        private static GeneralMatrix _matY {
            get {
                var y = new double[,] {
                    { 7, 8, },
                    { 9, 10, },
                    { 11, 12, }
                };
                return new GeneralMatrix(y);
            }
        }

        private static GeneralMatrix _matXY {
            get {
                var z = new double[,] {
                    { 58, 64, },
                    { 139, 154, }
                };
                return new GeneralMatrix(z);
            }
        }

        private static GeneralMatrix _singular {
            get {
                var z = new double[,] {
                    { 1, 2, 3, },
                    { 0, 1, 4, },
                    { 5, 6, 0, }
                };
                return new GeneralMatrix(z);
            }
        }

        private static GeneralMatrix _inverseSingular {
            get {
                var z = new double[,] {
                    { -24, 18, 5, },
                    { 20, -15, -4, },
                    { -5, 4, 1, }
                };
                return new GeneralMatrix(z);
            }
        }

        private static GeneralMatrix _sortable {
            get {
                var z = new double[,] {
                    { 0, 1, 0, },
                    { 0, 0, 1, },
                    { 1, 0, 0, },
                    { 1, 1, 0, },
                    { 0, 0, 0, },
                    { 1, 0, 1, },
                    { 1, 1, 1, },
                    { 0, 1, 1, },
                };
                return new GeneralMatrix(z);
            }
        }

        private static GeneralMatrix _sorted {
            get {
                var z = new double[,] {
                    { 0, 0, 0, },
                    { 0, 0, 1, },
                    { 0, 1, 0, },
                    { 0, 1, 1, },
                    { 1, 0, 0, },
                    { 1, 0, 1, },
                    { 1, 1, 0, },
                    { 1, 1, 1, },
                };
                return new GeneralMatrix(z);
            }
        }

        private static List<int> _sortedIndexes {
            get {
                var z = new List<int> { 4, 1, 0, 7, 2, 5, 3, 6 };
                return z;
            }
        }

        #endregion

        /// <summary>
        /// Test matrix initialization with a constant.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestInitialize() {
            var m = 100;
            var n = 200;
            var matrix = new GeneralMatrix(m, n, 0.5);
            Assert.AreEqual(m * n * 0.5, matrix.Sum(), double.Epsilon);
        }

        /// <summary>
        /// Test equal to itself. Should be true.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestIsEqual1() {
            Assert.IsTrue(_matX.IsEqual(_matX));
        }

        /// <summary>
        /// Test equal to a different matrix with the same dimensions. Should return false.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestIsEqual2() {
            var other = _matX.Multiply(3);
            Assert.IsFalse(_matX.IsEqual(other));
        }

        /// <summary>
        /// Test equal to matrix with different dimensions. Should return false.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestIsEqual3() {
            Assert.IsFalse(_matX.IsEqual(_matY));
        }

        /// <summary>
        /// Test equal to null: should return false.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestIsEqual4() {
            Assert.IsFalse(_matX.IsEqual(null));
        }

        /// <summary>
        /// Test multiplication with a factor.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestMultiplyFactor() {
            var m = 100;
            var n = 200;
            var matrix = new GeneralMatrix(m, n, 1);
            var multiplied = matrix.Multiply(0.5);
            Assert.AreEqual(m * n * 0.5, multiplied.Sum(), double.Epsilon);
        }

        /// <summary>
        /// Test multiplication of two matrices and compare with known result.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestMultiplyMatrix1() {
            var result = _matX.MultiplyOld(_matY);
            Assert.IsTrue(result.IsEqual(_matXY));
        }

        /// <summary>
        /// Test multiplication of two matrices and compare with known result.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestMultiplyMatrix2() {
            var result = _matX.MultiplyOld(_matY);
            Assert.IsTrue(result.IsEqual(_matX * _matY));
        }

        /// <summary>
        /// Test transpose and check inverse dimensions; test equal to double transpose.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestTranspose() {
            var mat = GeneralMatrix.Random(10,3);
            var resultTransposed = mat.Transpose();
            Assert.AreEqual(3, resultTransposed.RowDimension);
            Assert.AreEqual(10, resultTransposed.ColumnDimension);
            var resultTransposedTransposed = resultTransposed.Transpose();
            Assert.IsTrue(mat.IsEqual(resultTransposedTransposed));
        }

        /// <summary>
        /// Test copy / equal, modify, then unequal to original.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestCopy() {
            var mat = GeneralMatrix.Random(10, 3);
            var copy = mat.Copy();
            Assert.IsTrue(mat.IsEqual(copy));
            copy.Array[0][0] -= 1;
            Assert.IsFalse(mat.IsEqual(copy));
        }

        /// <summary>
        /// Test non-diagonal matrix: should return false.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestIsDiagonal1() {
            Assert.IsFalse(_matX.IsDiagonal());
        }

        /// <summary>
        /// Create identity matrix; test should yield true.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestIsDiagonal2() {
            var identity = GeneralMatrix.Identity(5,5);
            Assert.IsTrue(identity.IsDiagonal());
        }

        /// <summary>
        /// Create diagonal matrix, test if diagonal, modify non-diagonal cell, and test should return false.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestIsDiagonal3() {
            var diag = GeneralMatrix.CreateDiagonal(new double[] { 1, 2, 3, 4, 5 });
            Assert.IsTrue(diag.IsDiagonal());
            diag.Array[0][1] = 1e-4;
            Assert.IsFalse(diag.IsDiagonal());
        }

        /// <summary>
        /// Invert a singular matrix and test against known outcome. Inverting the inverse again should
        /// yield the original singular matrix.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_TestSingular() {
            var inverse = _singular.Inverse();
            Assert.IsTrue(inverse.IsApproximatelyEqual(_inverseSingular, 1e-4));
            Assert.IsTrue(inverse.Inverse().IsApproximatelyEqual(_singular, 1e-4));
        }

        [TestMethod]
        public void GeneralMatrixTest_TestMultiply2() {
            var m = 165;
            var n = 5;
            var c = 10000;
            var A = new GeneralMatrix(m, n, 1.0);
            var B = new GeneralMatrix(n, c, 1.0);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var C = A.MultiplyOld(B);
            var sum1 = C.ColumnPackedCopy.Sum();
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(sw.Elapsed);
        }

        [TestMethod]
        public void GeneralMatrixTest_TestSortMultiColumn1() {
            var sorted = _sortable.SortMultiColumn();
            _sortable.Print();
            sorted.Print();
            Assert.IsTrue(sorted.IsEqual(_sorted));
        }

        [TestMethod]
        public void GeneralMatrixTest_TestSortMultiColumn2() {
            var sortIndexes = _sortable.GetMultiColumnSortIndexes();
            _sortable.Print();
            CollectionAssert.AreEqual(sortIndexes, _sortedIndexes);
        }

        /// <summary>
        /// Initialization by means of delegate function: create a random matrix
        /// and fill a new matrix with a delegate that extracts elements from the
        /// random matrix. Assert whether the new matrix is equal to the initial.
        /// </summary>
        [TestMethod]
        public void GeneralMatrixTest_DelegateInitialization() {
            var matrix = GeneralMatrix.Random(100,100);
            Func<int,int,double> valueExtractor = (i, j) => matrix.Array[i][j];
            var matrix2 = new GeneralMatrix(matrix.RowDimension, matrix.ColumnDimension, valueExtractor);
            Assert.IsTrue(matrix.IsEqual(matrix2));
        }
    }
}
