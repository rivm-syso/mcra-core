using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MCRA.Utils {

    #region Internal Maths utility

    internal class Maths {

        /// <summary>
        /// sqrt(a^2 + b^2) without under/overflow.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Hypot(double a, double b) {
            double r;
            if (Math.Abs(a) > Math.Abs(b)) {
                r = b / a;
                r = Math.Abs(a) * Math.Sqrt(1 + r * r);
            } else if (b != 0) {
                r = a / b;
                r = Math.Abs(b) * Math.Sqrt(1 + r * r);
            } else {
                r = 0.0;
            }
            return r;
        }
    }

    /// <summary>
    /// Comparer class for sorting rows.
    /// </summary>
    internal class ColumnVectorComparer : IComparer<double[]> {
        public int Compare(double[] x, double[] y) {
            if (x.Length != y.Length) {
                throw new ArgumentException("Array lengths must equal.");
            }
            for (int i = 0; i < x.Length; i++) {
                if (x[i] > y[i]) {
                    return 1;
                } else if (x[i] < y[i]) {
                    return -1;
                }
            }
            return 0;
        }
    }

    #endregion

    /// <summary>.NET GeneralMatrix class.
    /// The .NET GeneralMatrix Class provides the fundamental operations of numerical
    /// linear algebra.  Various constructors create Matrices from two dimensional
    /// arrays of double precision floating point numbers.  Various "gets" and
    /// "sets" provide access to submatrices and matrix elements.  Several methods 
    /// implement basic matrix arithmetic, including matrix addition and
    /// multiplication, matrix norms, and element-by-element array operations.
    /// Methods for reading and printing matrices are also included.  All the
    /// operations in this version of the GeneralMatrix Class involve real matrices.
    /// Complex matrices may be handled in a future version.
    /// 
    /// Five fundamental matrix decompositions, which consist of pairs or triples
    /// of matrices, permutation vectors, and the like, produce results in five
    /// decomposition classes.  These decompositions are accessed by the GeneralMatrix
    /// class to compute solutions of simultaneous linear equations, determinants,
    /// inverses and other matrix functions.  The five decompositions are:
    /// <P><UL>
    /// <LI>Cholesky Decomposition of symmetric, positive definite matrices.
    /// <LI>LU Decomposition of rectangular matrices.
    /// <LI>QR Decomposition of rectangular matrices.
    /// <LI>Singular Value Decomposition of rectangular matrices.
    /// <LI>Eigenvalue Decomposition of both symmetric and nonsymmetric square matrices.
    /// </UL>
    /// <DL>
    /// <DT><B>Example of use:</B></DT>
    /// <P>
    /// <DD>Solve a linear system A x = b and compute the residual norm, ||b - A x||.
    /// <P><PRE>
    /// double[][] vals = {{1.,2.,3},{4.,5.,6.},{7.,8.,10.}};
    /// GeneralMatrix A = new GeneralMatrix(vals);
    /// GeneralMatrix b = GeneralMatrix.Random(3,1);
    /// GeneralMatrix x = A.Solve(b);
    /// GeneralMatrix r = A.Multiply(x).Subtract(b);
    /// double rnorm = r.NormInf();
    /// </PRE></DD>
    /// </DL>
    /// </summary>
    /// <author>  
    /// The MathWorks, Inc. and the National Institute of Standards and Technology.
    /// </author>
    /// <version>  5 August 1998
    /// </version>
    [Serializable]
    public sealed class GeneralMatrix : ICloneable, ISerializable, IDisposable {

        #region Class variables

        /// <summary>
        /// Array for internal storage of elements.
        /// </summary>
        private readonly double[][] _values;

        /// <summary>
        /// Row dimension.
        /// </summary>
        private readonly int _rows;

        /// <summary>
        /// Column dimension.
        /// </summary>
        private readonly int _columns;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct an m-by-n matrix of zeros.
        /// </summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of colums.</param>
        public GeneralMatrix(int m, int n) {
            _rows = m;
            _columns = n;
            _values = new double[m][];
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    _values[ix] = new double[n];
                });
        }

        /// <summary>Construct an m-by-n constant matrix.</summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of colums.</param>
        /// <param name="s">Fill the matrix with this scalar value.</param>
        public GeneralMatrix(int m, int n, double s) {
            _rows = m;
            _columns = n;
            _values = new double[m][];
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    _values[ix] = new double[n];
                    for (int j = 0; j < n; j++) {
                        _values[ix][j] = s;
                    }
                });
        }

        /// <summary>
        /// Construct a matrix from a 2-D jagged array.
        /// </summary>
        /// <param name="A">Two-dimensional array of doubles.</param>
        public GeneralMatrix(double[][] A) {
            _rows = A.Length;
            _columns = A[0].Length;
            for (int i = 0; i < _rows; i++) {
                if (A[i].Length != _columns) {
                    throw new System.ArgumentException("All rows must have the same length.");
                }
            }
            _values = A;
        }

        /// <summary>
        /// Construct a matrix from a 2-D multidimensional array.
        /// </summary>
        /// <param name="A"></param>
        public GeneralMatrix(double[,] A) {
            _rows = A.GetLength(0);
            _columns = A.GetLength(1);
            _values = new double[_rows][];
            for (int i = 0; i < _rows; i++) {
                _values[i] = new double[_columns];
                for (int j = 0; j < _columns; j++) {
                    _values[i][j] = A[i, j];
                }
            }
        }

        /// <summary>Construct a matrix quickly without checking arguments.</summary>
        /// <param name="A">Two-dimensional array of doubles.</param>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of colums.</param>
        public GeneralMatrix(double[][] A, int m, int n) {
            _values = A;
            _rows = m;
            _columns = n;
        }

        /// <summary>Construct a matrix from a one-dimensional packed array</summary>
        /// <param name="vals">One-dimensional array of doubles, packed by columns (ala Fortran).</param>
        /// <param name="m">Number of rows.</param>
        /// <exception cref="System.ArgumentException">Array length must be a multiple of m.</exception>
        public GeneralMatrix(double[] vals, int m) {
            _rows = m;
            _columns = (m != 0 ? vals.Length / m : 0);
            if (m * _columns != vals.Length) {
                throw new System.ArgumentException("Array length must be a multiple of m.");
            }
            _values = new double[m][];
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    _values[ix] = new double[_columns];
                    for (int j = 0; j < _columns; j++) {
                        _values[ix][j] = vals[ix + j * m];
                    }
                });
        }

        /// <summary>
        /// Initializes a general matrix using a cell value extraction delegate function.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="cellValueExtractor"></param>
        public GeneralMatrix(int m, int n, Func<int, int, double> cellValueExtractor) {
            _rows = m;
            _columns = n;
            _values = new double[_rows][];
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    _values[ix] = new double[_columns];
                    for (int j = 0; j < n; j++) {
                        _values[ix][j] = cellValueExtractor(ix, j);
                    }
                });
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Access the internal two-dimensional array.
        /// </summary>
        /// <returns>Pointer to the two-dimensional array of matrix elements.</returns>
        public double[][] Array {
            get {
                return _values;
            }
        }

        /// <summary>Copy the internal two-dimensional array.</summary>
        /// <returns>Two-dimensional array copy of matrix elements.</returns>
        public double[][] ArrayCopy {
            get {
                var copy = new double[_rows][];
                for (int i = 0; i < _rows; i++) {
                    copy[i] = new double[_columns];
                    for (int j = 0; j < _columns; j++) {
                        copy[i][j] = _values[i][j];
                    }
                }
                return copy;
            }
        }

        /// <summary>
        /// Copy the internal two-dimensional array.
        /// </summary>
        /// <returns>Two-dimensional array copy of matrix elements.</returns>
        public double[,] ArrayCopy2 {
            get {
                var copy = new double[_rows, _columns];
                for (int i = 0; i < _rows; i++) {
                    for (int j = 0; j < _columns; j++) {
                        copy[i, j] = _values[i][j];
                    }
                }

                return copy;
            }
        }

        /// <summary>
        /// Copy the transpose internal two-dimensional array.
        /// </summary>
        /// <returns>Two-dimensional transposed array copy of matrix elements.</returns>
        public double[,] TransposeArrayCopy2 {
            get {
                var copy = new double[_columns, _rows];
                for (int i = 0; i < _rows; i++) {
                    for (int j = 0; j < _columns; j++) {
                        copy[j, i] = _values[i][j];
                    }
                }
                return copy;
            }
        }

        /// <summary>
        /// Make a one-dimensional column packed copy of the internal array.
        /// </summary>
        /// <returns>Matrix elements packed in a one-dimensional array by columns.</returns>
        public double[] ColumnPackedCopy {
            get {
                var vals = new double[_rows * _columns];
                Enumerable.Range(0, _rows)
                    .AsParallel()
                    .ForAll((ix) => {
                        for (int j = 0; j < _columns; j++) {
                            vals[ix + j * _rows] = _values[ix][j];
                        }
                    });
                return vals;
            }
        }

        /// <summary>
        /// Make a one-dimensional row packed copy of the internal array.
        /// </summary>
        /// <returns>Matrix elements packed in a one-dimensional array by rows.</returns>
        public double[] RowPackedCopy {
            get {
                var values = new double[_rows * _columns];
                for (int i = 0; i < _rows; i++) {
                    for (int j = 0; j < _columns; j++) {
                        values[i * _columns + j] = _values[i][j];
                    }
                }
                return values;
            }
        }

        /// <summary>Get row dimension.</summary>
        /// <returns>m, the number of rows.</returns>
        public int RowDimension {
            get {
                return _rows;
            }
        }

        /// <summary>Get column dimension.</summary>
        /// <returns>n, the number of columns.</returns>
        public int ColumnDimension {
            get {
                return _columns;
            }
        }

        #endregion

        #region Static instance creators

        /// <summary>Generate matrix with random elements</summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of colums.</param>
        /// <returns>An m-by-n matrix with uniformly distributed random elements.</returns>
        public static GeneralMatrix Random(int m, int n) {
            IRandom random = new McraRandomGenerator();
            var A = new GeneralMatrix(m, n);
            var X = A.Array;
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < n; j++) {
                    X[i][j] = random.NextDouble();
                }
            }
            return A;
        }

        /// <summary>Generate identity matrix</summary>
        /// <param name="m">Number of rows.</param>
        /// <param name="n">Number of colums.</param>
        /// <returns>An m-by-n matrix with ones on the diagonal and zeros elsewhere.</returns>
        public static GeneralMatrix Identity(int m, int n) {
            var identity = new GeneralMatrix(m, n);
            double[][] X = identity.Array;
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < n; j++) {
                    X[i][j] = (i == j ? 1.0 : 0.0);
                }
            }
            return identity;
        }

        /// <summary>
        /// Construct a diagonal symmetric matrix from a copy of a 1-D array.
        /// </summary>
        /// <param name="diagonal">Diagonal Elements.</param>
        public static GeneralMatrix CreateDiagonal(double[] diagonal) {
            int m = diagonal.Length;
            var x = new GeneralMatrix(m, m);
            var C = x.Array;
            for (int i = 0; i < m; i++) {
                for (int j = 0; j < m; j++) {
                    if (i == j) {
                        C[i][j] = diagonal[i];
                    } else {
                        C[i][j] = 0.0;
                    }
                }
            }
            return x;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Matrix transpose.
        /// </summary>
        /// <returns>A'</returns>
        public GeneralMatrix Transpose() {
            var x = new GeneralMatrix(_columns, _rows);
            double[][] C = x.Array;
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        C[j][ix] = _values[ix][j];
                    }
                });
            return x;
        }

        /// <summary>
        /// Make a deep copy of a matrix while replacing NaN
        /// </summary>
        /// <param name="replace">Value to replace NaN</param>
        public GeneralMatrix NanReplace(double replace) {
            var x = new GeneralMatrix(_rows, _columns);
            double[][] C = x.Array;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    if (double.IsNaN(_values[i][j])) {
                        C[i][j] = replace;
                    } else {
                        C[i][j] = _values[i][j];
                    }
                }
            }
            return x;
        }

        /// <summary>
        /// Make a deep copy of a matrix while inserting NaN
        /// </summary>
        /// <param name="replace">Value which is replace by NaN</param>
        public GeneralMatrix NanInsert(double replace) {
            var x = new GeneralMatrix(_rows, _columns);
            double[][] C = x.Array;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    if (_values[i][j] == replace) {
                        C[i][j] = double.NaN;
                    } else {
                        C[i][j] = _values[i][j];
                    }
                }
            }
            return x;
        }

        /// <summary>
        /// Make a deep copy of a matrix while inserting NaN 
        /// </summary>
        /// <param name="replace">Specifies which elements must be set to NaN.</param>
        /// <exception cref="System.ArgumentException">Matrices must be compatible
        /// </exception>
        public GeneralMatrix NanInsert(GeneralMatrix replace) {
            var x = new GeneralMatrix(_rows, _columns);
            double[][] C = x.Array;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    if (double.IsNaN(replace.GetElement(i, j))) {
                        C[i][j] = double.NaN;
                    } else {
                        C[i][j] = _values[i][j];
                    }
                }
            }
            return x;
        }

        /// <summary>
        /// Make a deep copy of a matrix.
        /// </summary>
        public GeneralMatrix Copy() {
            var X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    C[i][j] = _values[i][j];
                }
            }
            return X;
        }

        /// <summary>Get a single element.</summary>
        /// <param name="i">   Row index.
        /// </param>
        /// <param name="j">   Column index.
        /// </param>
        /// <returns>     A(i,j)
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">  
        /// </exception>
        public double GetElement(int i, int j) {
            return _values[i][j];
        }

        /// <summary>Get a submatrix.</summary>
        /// <param name="i0">Initial row index.</param>
        /// <param name="i1">Final row index</param>
        /// <param name="j0">Initial column index</param>
        /// <param name="j1">Final column index</param>
        /// <returns>A(i0:i1,j0:j1)</returns>
        /// <exception cref="System.IndexOutOfRangeException">Submatrix indices</exception>
        public GeneralMatrix GetSubMatrix(int i0, int i1, int j0, int j1) {
            GeneralMatrix X = new GeneralMatrix(i1 - i0 + 1, j1 - j0 + 1);
            double[][] B = X.Array;
            try {
                for (int i = i0; i <= i1; i++) {
                    for (int j = j0; j <= j1; j++) {
                        B[i - i0][j - j0] = _values[i][j];
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
            return X;
        }

        /// <summary>
        /// Get a submatrix.
        /// </summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="c">Array of column indices.</param>
        /// <returns>A(r(:),c(:))</returns>
        /// <exception cref="System.IndexOutOfRangeException">Submatrix indices</exception>
        public GeneralMatrix GetMatrix(int[] r, int[] c) {
            GeneralMatrix X = new GeneralMatrix(r.Length, c.Length);
            double[][] B = X.Array;
            try {
                for (int i = 0; i < r.Length; i++) {
                    for (int j = 0; j < c.Length; j++) {
                        B[i][j] = _values[r[i]][c[j]];
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
            return X;
        }

        /// <summary>
        /// Get a submatrix.
        /// </summary>
        /// <param name="i0">Initial row index</param>
        /// <param name="i1">Final row index</param>
        /// <param name="c">Array of column indices.</param>
        /// <returns>A(i0:i1,c(:))</returns>
        /// <exception cref="System.IndexOutOfRangeException">Submatrix indices</exception>
        public GeneralMatrix GetMatrix(int i0, int i1, int[] c) {
            GeneralMatrix X = new GeneralMatrix(i1 - i0 + 1, c.Length);
            double[][] B = X.Array;
            try {
                for (int i = i0; i <= i1; i++) {
                    for (int j = 0; j < c.Length; j++) {
                        B[i - i0][j] = _values[i][c[j]];
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
            return X;
        }

        /// <summary>
        /// Get a submatrix.
        /// </summary>
        /// <param name="r">Array of row indices.</param>
        /// <param name="j0">Initial column index</param>
        /// <param name="j1">Final column index.</param>
        /// <returns>A(r(:),j0:j1)</returns>
        /// <exception cref="System.IndexOutOfRangeException">Submatrix indices</exception>
        public GeneralMatrix GetMatrix(int[] r, int j0, int j1) {
            GeneralMatrix X = new GeneralMatrix(r.Length, j1 - j0 + 1);
            double[][] B = X.Array;
            try {
                for (int i = 0; i < r.Length; i++) {
                    for (int j = j0; j <= j1; j++) {
                        B[i][j - j0] = _values[r[i]][j];
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
            return X;
        }

        /// <summary>
        /// Set a single element.
        /// </summary>
        /// <param name="i">Row index.</param>
        /// <param name="j">Column index.</param>
        /// <param name="s">A(i,j).</param>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public void SetElement(int i, int j, double s) {
            _values[i][j] = s;
        }

        /// <summary>Set a submatrix.</summary>
        /// <param name="i0">  Initial row index
        /// </param>
        /// <param name="i1">  Final row index
        /// </param>
        /// <param name="j0">  Initial column index
        /// </param>
        /// <param name="j1">  Final column index
        /// </param>
        /// <param name="X">   A(i0:i1,j0:j1)
        /// </param>
        /// <exception cref="System.IndexOutOfRangeException">  Submatrix indices
        /// </exception>
        public void SetMatrix(int i0, int i1, int j0, int j1, GeneralMatrix X) {
            try {
                for (int i = i0; i <= i1; i++) {
                    for (int j = j0; j <= j1; j++) {
                        _values[i][j] = X.GetElement(i - i0, j - j0);
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
        }

        /// <summary>Set a submatrix.</summary>
        /// <param name="r">   Array of row indices.
        /// </param>
        /// <param name="c">   Array of column indices.
        /// </param>
        /// <param name="X">   A(r(:),c(:))
        /// </param>
        /// <exception cref="System.IndexOutOfRangeException">  Submatrix indices
        /// </exception>
        public void SetMatrix(int[] r, int[] c, GeneralMatrix X) {
            try {
                for (int i = 0; i < r.Length; i++) {
                    for (int j = 0; j < c.Length; j++) {
                        _values[r[i]][c[j]] = X.GetElement(i, j);
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
        }

        /// <summary>Set a submatrix.</summary>
        /// <param name="r">   Array of row indices.
        /// </param>
        /// <param name="j0">  Initial column index
        /// </param>
        /// <param name="j1">  Final column index
        /// </param>
        /// <param name="X">   A(r(:),j0:j1)
        /// </param>
        /// <exception cref="System.IndexOutOfRangeException"> Submatrix indices
        /// </exception>
        public void SetMatrix(int[] r, int j0, int j1, GeneralMatrix X) {
            try {
                for (int i = 0; i < r.Length; i++) {
                    for (int j = j0; j <= j1; j++) {
                        _values[r[i]][j] = X.GetElement(i, j - j0);
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
        }

        /// <summary>Set a submatrix.</summary>
        /// <param name="i0">  Initial row index
        /// </param>
        /// <param name="i1">  Final row index
        /// </param>
        /// <param name="c">   Array of column indices.
        /// </param>
        /// <param name="X">   A(i0:i1,c(:))
        /// </param>
        /// <exception cref="System.IndexOutOfRangeException">  Submatrix indices
        /// </exception>
        public void SetMatrix(int i0, int i1, int[] c, GeneralMatrix X) {
            try {
                for (int i = i0; i <= i1; i++) {
                    for (int j = 0; j < c.Length; j++) {
                        _values[i][c[j]] = X.GetElement(i - i0, j);
                    }
                }
            } catch (System.IndexOutOfRangeException e) {
                throw new System.IndexOutOfRangeException("Submatrix indices", e);
            }
        }

        /// <summary>
        /// One norm
        /// </summary>
        /// <returns>Maximum column sum.</returns>
        public double Norm1() {
            double f = 0;
            for (int j = 0; j < _columns; j++) {
                double s = 0;
                for (int i = 0; i < _rows; i++) {
                    s += System.Math.Abs(_values[i][j]);
                }
                f = System.Math.Max(f, s);
            }
            return f;
        }

        /// <summary>Two norm</summary>
        /// <returns>Maximum singular value.</returns>
        public double Norm2() {
            return (new SingularValueDecomposition(this).Norm2());
        }

        /// <summary>Infinity norm</summary>
        /// <returns>Maximum row sum.</returns>
        public double NormInf() {
            double f = 0;
            for (int i = 0; i < _rows; i++) {
                double s = 0;
                for (int j = 0; j < _columns; j++) {
                    s += System.Math.Abs(_values[i][j]);
                }
                f = System.Math.Max(f, s);
            }
            return f;
        }

        /// <summary>
        /// Frobenius norm
        /// </summary>
        /// <returns>sqrt of sum of squares of all elements.</returns>
        public double NormF() {
            double f = 0;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    f = Maths.Hypot(f, _values[i][j]);
                }
            }
            return f;
        }

        /// <summary>
        /// Unary minus
        /// </summary>
        /// <returns>-A</returns>
        public GeneralMatrix UnaryMinus() {
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    C[i][j] = -_values[i][j];
                }
            }
            return X;
        }

        /// <summary>C = A + B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>A + B</returns>
        public GeneralMatrix Add(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    C[i][j] = _values[i][j] + B._values[i][j];
                }
            }
            return X;
        }

        /// <summary>
        /// A = A + B
        /// </summary>
        /// <param name="B">another matrix.</param>
        /// <returns>A + B</returns>
        public GeneralMatrix AddAssign(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    _values[i][j] = _values[i][j] + B._values[i][j];
                }
            }
            return this;
        }

        /// <summary>
        /// C = A - B
        /// </summary>
        /// <param name="B">another matrix</param>
        /// <returns>A - B</returns>
        public GeneralMatrix Subtract(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            Enumerable.Range(0, _rows)
              .AsParallel()
              .ForAll((ix) => {
                  //for (int i = 0; i < _rows; i++) {
                  for (int j = 0; j < _columns; j++) {
                      C[ix][j] = _values[ix][j] - B._values[ix][j];
                  }
              });
            return X;
        }

        /// <summary>
        /// A = A - s
        /// </summary>
        /// <param name="B">a double</param>
        /// <returns>A - s</returns>
        public GeneralMatrix SubtractAssign(double s) {
            Enumerable.Range(0, _rows)
              .AsParallel()
              .ForAll((ix) => {
                  //for (int i = 0; i < _rows; i++) {
                  for (int j = 0; j < _columns; j++) {
                      _values[ix][j] = _values[ix][j] - s;
                  }
              });
            return this;
        }

        /// <summary>
        /// A = A + s
        /// </summary>
        /// <param name="B">a double</param>
        /// <returns>A + s</returns>
        public GeneralMatrix AddAssign(double s) {
            Enumerable.Range(0, _rows)
              .AsParallel()
              .ForAll((ix) => {
                  //for (int i = 0; i < _rows; i++) {
                  for (int j = 0; j < _columns; j++) {
                      _values[ix][j] = _values[ix][j] + s;
                  }
              });
            return this;
        }

        /// <summary>
        /// A = A + s
        /// </summary>
        /// <param name="B">a double</param>
        /// <returns>A + s</returns>
        public GeneralMatrix ReplaceNegativeAssign() {
            Enumerable.Range(0, _rows)
            .AsParallel()
            .ForAll((ix) => {
                for (int j = 0; j < _columns; j++) {
                    _values[ix][j] = Math.Max(0, _values[ix][j]);
                }
            });
            return this;
        }

        /// <summary>
        /// A = A - B
        /// </summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A - B</returns>
        public GeneralMatrix SubtractAssign(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            Enumerable.Range(0, _rows)
              .AsParallel()
              .ForAll((ix) => {
                  //for (int i = 0; i < _rows; i++) {
                  for (int j = 0; j < _columns; j++) {
                      _values[ix][j] = _values[ix][j] - B._values[ix][j];
                  }
              });
            return this;
        }

        /// <summary>
        /// Element-by-element multiplication, C = A.*B
        /// </summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A.*B</returns>
        public GeneralMatrix ArrayMultiply(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        C[ix][j] = _values[ix][j] * B._values[ix][j];
                    }
                });
            return X;
        }

        /// <summary>
        /// Element-by-element multiplication in place, A = A.*B
        /// </summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A.*B</returns>
        public GeneralMatrix ArrayMultiplyAssign(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        _values[ix][j] = _values[ix][j] * B._values[ix][j];
                    }
                });
            return this;
        }

        /// <summary>
        /// Element-by-element right division, C = A./B
        /// </summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A./B</returns>
        public GeneralMatrix ArrayRightDivide(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        C[ix][j] = _values[ix][j] / B._values[ix][j];
                    }
                });
            return X;
        }

        /// <summary>
        /// Element-by-element right division in place, A = A./B
        /// </summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A./B
        /// </returns>
        public GeneralMatrix ArrayRightDivideAssign(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        _values[ix][j] = _values[ix][j] / B._values[ix][j];
                    }
                });
            return this;
        }

        /// <summary>Element-by-element left division, C = A.\B</summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A.\B</returns>
        public GeneralMatrix ArrayLeftDivide(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        C[ix][j] = B._values[ix][j] / _values[ix][j];
                    }
                });
            return X;
        }

        /// <summary>Element-by-element left division in place, A = A.\B</summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A.\B</returns>
        public GeneralMatrix ArrayLeftDivideAssign(GeneralMatrix B) {
            CheckMatrixDimensions(B);
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        _values[ix][j] = B._values[ix][j] / _values[ix][j];
                    }
                });
            return this;
        }

        /// <summary>Element-by-element left division in place, A = A/s</summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A.\B</returns>
        public GeneralMatrix ArrayRightDivideAssign(double s) {
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        _values[ix][j] = _values[ix][j] / s;
                    }
                });
            return this;
        }

        /// <summary>Element-by-element left division in place, A = A/s</summary>
        /// <param name="B">Another matrix.</param>
        /// <returns>A.\B</returns>
        public GeneralMatrix ArrayRightDivide(double s) {
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        C[ix][j] = _values[ix][j] / s;
                    }
                });
            return X;
        }

        /// <summary>
        /// Multiply a matrix by a scalar, C = s*A.
        /// </summary>
        /// <param name="s">scalar</param>
        /// <returns>s*A</returns>
        public GeneralMatrix Multiply(double s) {
            GeneralMatrix X = new GeneralMatrix(_rows, _columns);
            double[][] C = X.Array;
            Enumerable.Range(0, _rows)
               .AsParallel()
               .ForAll((ix) => {
                   for (int j = 0; j < _columns; j++) {
                       C[ix][j] = s * _values[ix][j];
                   }
               });
            return X;
        }

        /// <summary>Multiply a matrix by a scalar in place, A = s*A</summary>
        /// <param name="s">scalar</param>
        /// <returns>replace A by s*A</returns>
        public GeneralMatrix MultiplyAssign(double s) {
            Enumerable.Range(0, _rows)
                .AsParallel()
                .ForAll((ix) => {
                    for (int j = 0; j < _columns; j++) {
                        _values[ix][j] = s * _values[ix][j];
                    }
                });
            return this;
        }

        /// <summary>Linear algebraic matrix multiplication, A * B</summary>
        /// <param name="B">another matrix</param>
        /// <returns>Matrix product, A * B</returns>
        /// <exception cref="System.ArgumentException">Matrix inner dimensions must agree.</exception>
        public GeneralMatrix MultiplyOld(GeneralMatrix B) {
            if (B._rows != _columns) {
                throw new System.ArgumentException("GeneralMatrix inner dimensions must agree.");
            }
            GeneralMatrix X = new GeneralMatrix(_rows, B._columns);
            double[][] C = X.Array;
            double[] Bcolj = new double[_columns];
            for (int j = 0; j < B._columns; j++) {
                for (int k = 0; k < _columns; k++) {
                    Bcolj[k] = B._values[k][j];
                }
                for (int i = 0; i < _rows; i++) {
                    double[] Arowi = _values[i];
                    double s = 0;
                    for (int k = 0; k < _columns; k++) {
                        s += Arowi[k] * Bcolj[k];
                    }
                    C[i][j] = s;
                }
            }
            return X;
        }

        public GeneralMatrix Multiply(GeneralMatrix B) {
            if (B._rows != _columns) {
                throw new System.ArgumentException("GeneralMatrix inner dimensions must agree.");
            }
            var X = new GeneralMatrix(_rows, B._columns);
            double[][] C = X.Array;

            var tB = new double[B._columns][];
            Enumerable.Range(0, B.ColumnDimension)
                .AsParallel()
                .ForAll(ix => {
                    tB[ix] = new double[B._rows];
                    for (int i = 0; i < B._rows; i++) {
                        tB[ix][i] = B.Array[i][ix];
                    }
                });

            Enumerable.Range(0, B.ColumnDimension)
                .AsParallel()
                .ForAll(ix => {
                    for (int i = 0; i < _rows; i++) {
                        double[] Arowi = _values[i];
                        double s = 0;
                        for (int k = 0; k < _columns; k++) {
                            s += Arowi[k] * tB[ix][k];
                        }
                        C[i][ix] = s;
                    }
                });
            return X;
        }

        /// <summary>
        /// Sortes the rows based on the column values. Sorts first on the first column values,
        /// then on the second column, etc.
        /// </summary>
        /// <returns></returns>
        public GeneralMatrix SortMultiColumn() {
            var sorted = this.Copy();
            System.Array.Sort(sorted.Array, new ColumnVectorComparer());
            return sorted;
        }

        /// <summary>
        /// Returns the row indexes (i.e., the arrengement of elements) of the ordered
        /// rows when sorting on the column values.
        /// </summary>
        /// <returns></returns>
        public List<int> GetMultiColumnSortIndexes() {
            var sorted = Array
                .Select((x, i) => new KeyValuePair<double[], int>(x, i))
                .OrderBy(x => x.Key, new ColumnVectorComparer())
                .Select(x => x.Value)
                .ToList();
            return sorted;
        }

        #region Operator Overloading

        /// <summary>
        ///  Addition of matrices
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static GeneralMatrix operator +(GeneralMatrix m1, GeneralMatrix m2) {
            return m1.Add(m2);
        }

        /// <summary>
        /// Subtraction of matrices
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static GeneralMatrix operator -(GeneralMatrix m1, GeneralMatrix m2) {
            return m1.Subtract(m2);
        }

        /// <summary>
        /// Multiplication of matrices
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static GeneralMatrix operator *(GeneralMatrix m1, GeneralMatrix m2) {
            return m1.MultiplyOld(m2);
        }

        #endregion

        /// <summary>LU Decomposition</summary>
        /// <returns>LUDecomposition</returns>
        /// <seealso cref="LUDecomposition"></seealso>
        public LUDecomposition LUD() {
            return new LUDecomposition(this);
        }

        /// <summary>
        /// QR Decomposition
        /// </summary>
        /// <returns>QRDecomposition</returns>
        /// <seealso cref="QRDecomposition"></seealso>
        public QRDecomposition QRD() {
            return new QRDecomposition(this);
        }

        /// <summary>
        /// Cholesky Decomposition
        /// </summary>
        /// <returns>CholeskyDecomposition</returns>
        /// <seealso cref="CholeskyDecomposition"></seealso>
        public CholeskyDecomposition chol() {
            return new CholeskyDecomposition(this);
        }

        /// <summary>Singular Value Decomposition</summary>
        /// <returns>SingularValueDecomposition</returns>
        /// <seealso cref="SingularValueDecomposition">
        /// </seealso>
        public SingularValueDecomposition SVD() {
            return new SingularValueDecomposition(this);
        }

        /// <summary>
        /// Eigenvalue Decomposition
        /// </summary>
        /// <returns>Eigenvalue Decomposition</returns>
        /// <seealso cref="EigenvalueDecomposition"></seealso>
        public EigenvalueDecomposition Eigen() {
            return new EigenvalueDecomposition(this);
        }

        /// <summary>Solve A*X = B</summary>
        /// <param name="B">   right hand side
        /// </param>
        /// <returns>Solution if A is square, least squares solution otherwise.</returns>
        public GeneralMatrix Solve(GeneralMatrix B) {
            return (_rows == _columns ? (new LUDecomposition(this)).Solve(B) : (new QRDecomposition(this)).Solve(B));
        }

        /// <summary>Solve X*A = B, which is also A'*X' = B'</summary>
        /// <param name="B">Right hand side</param>
        /// <returns>Solution if A is square, least squares solution otherwise.</returns>
        public GeneralMatrix SolveTranspose(GeneralMatrix B) {
            return Transpose().Solve(B.Transpose());
        }

        /// <summary>Matrix inverse or pseudoinverse</summary>
        /// <returns>inverse(A) if A is square, pseudoinverse otherwise.</returns>
        public GeneralMatrix Inverse() {
            return Solve(Identity(_rows, _rows));
        }

        /// <summary>
        /// Gets the determinant.
        /// </summary>
        /// <returns>The determinant.</returns>
        public double Determinant() {
            return new LUDecomposition(this).Determinant();
        }

        /// <summary>GeneralMatrix rank</summary>
        /// <returns>Effective numerical rank, obtained from SVD.</returns>
        public int Rank() {
            return new SingularValueDecomposition(this).Rank();
        }

        /// <summary>
        /// Matrix condition (2 norm).
        /// </summary>
        /// <returns>Ratio of largest to smallest singular value.</returns>
        public double Condition() {
            return new SingularValueDecomposition(this).Condition();
        }

        /// <summary>
        /// Matrix trace.
        /// </summary>
        /// <returns>Sum of the diagonal elements.</returns>
        public double Trace() {
            double t = 0;
            for (int i = 0; i < System.Math.Min(_rows, _columns); i++) {
                t += _values[i][i];
            }
            return t;
        }

        /// <summary>
        /// Matrix cell element sum.
        /// </summary>
        /// <returns>Sum of all elements.</returns>
        public double Sum() {
            var t = 0D;
            for (int i = 0; i < _rows; i++) {
                for (int j = 0; j < _columns; j++) {
                    t += _values[i][j];
                }
            }
            return t;
        }

        /// <summary>
        /// Checks whether the other matrix is equal to this matrix.
        /// Returns true if this is the case, otherwise false.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEqual(GeneralMatrix other) {
            if (other == null
                || this.RowDimension != other.RowDimension
                || this.ColumnDimension != other.ColumnDimension) {
                return false;
            }
            for (int i = 0; i < RowDimension; i++) {
                for (int j = 0; j < ColumnDimension; j++) {
                    if (_values[i][j] != other.Array[i][j]) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks whether the other matrix is equal to this matrix.
        /// Returns true if this is the case, otherwise false.
        /// </summary>
        /// <param name="other">The other matrix.</param>
        /// <param name="epsilon">The tolerance factor of being approximately equal.</param>
        /// <returns></returns>
        public bool IsApproximatelyEqual(GeneralMatrix other, double epsilon) {
            if (other == null
                || this.RowDimension != other.RowDimension
                || this.ColumnDimension != other.ColumnDimension) {
                return false;
            }
            for (int i = 0; i < RowDimension; i++) {
                for (int j = 0; j < ColumnDimension; j++) {
                    if (Math.Abs(_values[i][j] - other.Array[i][j]) > epsilon) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks whether the other matrix is square.
        /// </summary>
        /// <returns></returns>
        public bool IsSquare() {
            return (_rows == _columns);
        }

        /// <summary>
        /// Checks whether the other matrix is equal to this matrix.
        /// Returns true if this is the case, otherwise false.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsDiagonal() {
            if (!IsSquare()) {
                return false;
            }
            for (int i = 0; i < RowDimension; i++) {
                for (int j = 0; j < ColumnDimension; j++) {
                    if ((i != j && _values[i][j] != 0D)) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Prints the matrix for to the debug trace.
        /// </summary>
        public void Print() {
            Debug.Write("[ ");
            for (int i = 0; i < RowDimension; i++) {
                if (i > 0) {
                    Debug.Write("  ");
                }
                for (int j = 0; j < ColumnDimension; j++) {
                    Debug.Write(message: $"{_values[i][j]:G3} ");
                }
                if (i < RowDimension - 1) {
                    Debug.WriteLine("");
                }
            }
            Debug.WriteLine("]");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if size(A) == size(B).
        /// </summary>
        /// <param name="B"></param>
        private void CheckMatrixDimensions(GeneralMatrix B) {
            if (B._rows != _rows || B._columns != _columns) {
                throw new System.ArgumentException("GeneralMatrix dimensions must agree.");
            }
        }

        #endregion

        #region Implement IDisposable

        /// <summary>
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing) {
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            if (disposing) {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// This destructor will run only if the Dispose method 
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </summary>
        ~GeneralMatrix() {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        /// <summary>Clone the GeneralMatrix object.</summary>
        public System.Object Clone() {
            return this.Copy();
        }

        /// <summary>
        /// A method called when serializing this class
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
        }

        /// <summary>
        /// The k x v matrix  is normalised per colum, e.g. values are divided 
        /// by the total sum of each column.
        /// </summary>
        /// <returns></returns>
        public GeneralMatrix NormalizeColumns() {
            var tMatrix = this.Transpose();
            var norms = tMatrix.Array.Select(c => c.Sum()).ToArray();
            var _matSum = new GeneralMatrix(norms, this.ColumnDimension);
            var _matDesign = new GeneralMatrix(Enumerable.Repeat(1d, this.RowDimension).ToArray(), 1);
            return this.ArrayRightDivide(_matSum.MultiplyOld(_matDesign).Transpose());
        }

        /// <summary>
        /// The k x v matrix  is standardised per colum, e.g. values are divided 
        /// by the standard deviation each column.
        /// </summary>
        /// <returns></returns>
        public GeneralMatrix StandardizeColumns() {
            var sds = this.Array.Select(c => 1 / Math.Sqrt(c.Variance())).ToArray();
            var standardizeMatrix = this.MultiplyRows(sds);
            return standardizeMatrix;
        }
        /// <summary>
        /// Center en standardise: z = (y - mu)/sd
        /// </summary>
        /// <returns></returns>
        public GeneralMatrix ScaleRows() {
            var scaledMatrix = new GeneralMatrix(this.RowDimension, this.ColumnDimension);
            for (int i = 0; i < this.RowDimension; i++) {
                var row = this.Array[i].ToList();
                var mean = row.Average(c => c);
                var sd = Math.Sqrt(row.Variance());
                for (int j = 0; j < this.ColumnDimension; j++) {
                    scaledMatrix.Array[i][j] = (this.Array[i][j] - mean) / sd;
                }
            }
            return scaledMatrix;
        }

        /// <summary>
        /// Multiply the rows by a factor
        /// </summary>
        /// <returns></returns>
        public GeneralMatrix MultiplyRows(double[] factor) {
            if (factor.Length != this.RowDimension) {
                throw new System.ArgumentException("GeneralMatrix dimensions must agree.");
            }
            var scaledMatrix = new GeneralMatrix(this.RowDimension, this.ColumnDimension);
            for (int i = 0; i < this.RowDimension; i++) {
                for (int j = 0; j < this.ColumnDimension; j++) {
                    scaledMatrix.Array[i][j] = this.Array[i][j] * factor[i];
                }
            }
            return scaledMatrix;
        }
        public string WriteToCsvFile(
            string filename,
            List<string> rowNames,
            List<string> colNames
        ) {
            using (var stream = new FileStream(filename, FileMode.Create)) {
                using (var streamWriter = new StreamWriter(stream, Encoding.Default)) {
                    streamWriter.WriteLine($",{string.Join(",", colNames)}");
                    for (int i = 0; i < rowNames.Count; i++) {
                        var row = new List<string>();
                        row.Add(rowNames[i]);
                        row.AddRange(this.Array[i].Select(c => c.ToString()));
                        streamWriter.WriteLine(string.Join(",", row));
                    }
                }
            }
            return filename;
        }
    }
}
