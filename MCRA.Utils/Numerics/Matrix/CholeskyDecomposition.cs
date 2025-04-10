﻿using System.Runtime.Serialization;

namespace MCRA.Utils {

    /// <summary>Cholesky Decomposition.
    /// For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.
    /// If the matrix is not symmetric or positive definite, the constructor
    /// returns a partial decomposition and sets an internal flag that may
    /// be queried by the isSPD() method.
    /// </summary>
    [Serializable]
    public class CholeskyDecomposition : ISerializable {

        #region Class variables

        /// <summary>Array for internal storage of decomposition.
        /// @serial internal array storage.
        /// </summary>
        private readonly double[][] _l;

        /// <summary>Row and column dimension (square matrix).
        /// @serial matrix dimension.
        /// </summary>
        private readonly int _n;

        /// <summary>Symmetric and positive definite flag.
        /// @serial is symmetric and positive definite flag.
        /// </summary>
        private readonly bool _isPositiveDefinite;

        #endregion

        #region Constructor

        /// <summary>Cholesky algorithm for symmetric and positive definite matrix.</summary>
        /// <param name="Arg">  Square, symmetric matrix.
        /// </param>
        /// <returns>
        /// Structure to access L and isspd flag.
        /// </returns>
        public CholeskyDecomposition(GeneralMatrix Arg) {
            // Initialize.
            double[][] A = Arg.Array;
            _n = Arg.RowDimension;
            _l = new double[_n][];
            for (int i = 0; i < _n; i++) {
                _l[i] = new double[_n];
            }
            _isPositiveDefinite = (Arg.ColumnDimension == _n);
            // Main loop.
            for (int j = 0; j < _n; j++) {
                double[] Lrowj = _l[j];
                double d = 0.0;
                for (int k = 0; k < j; k++) {
                    double[] Lrowk = _l[k];
                    double s = 0.0;
                    for (int i = 0; i < k; i++) {
                        s += Lrowk[i] * Lrowj[i];
                    }
                    Lrowj[k] = s = (A[j][k] - s) / _l[k][k];
                    d = d + s * s;
                    _isPositiveDefinite = _isPositiveDefinite && (A[k][j] == A[j][k]);
                }
                d = A[j][j] - d;
                _isPositiveDefinite = _isPositiveDefinite && (d > 0.0);
                _l[j][j] = System.Math.Sqrt(System.Math.Max(d, 0.0));
                for (int k = j + 1; k < _n; k++) {
                    _l[j][k] = 0.0;
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Is the matrix symmetric and positive definite?
        /// </summary>
        /// <returns>true if A is symmetric and positive definite.</returns>
        virtual public bool SPD {
            get {
                return _isPositiveDefinite;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Return triangular factor.
        /// </summary>
        /// <returns>L</returns>
        public virtual GeneralMatrix GetL() {
            return new GeneralMatrix(_l, _n, _n);
        }

        /// <summary>Solve A*X = B</summary>
        /// <param name="B">  A Matrix with as many rows as A and any number of columns.
        /// </param>
        /// <returns>
        /// X so that L*L'*X = B
        /// </returns>
        /// <exception cref="ArgumentException">  Matrix row dimensions must agree.
        /// </exception>
        /// <exception cref="SystemException"> Matrix is not symmetric positive definite.
        /// </exception>
        public virtual GeneralMatrix Solve(GeneralMatrix B) {
            if (B.RowDimension != _n) {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!_isPositiveDefinite) {
                throw new SystemException("Matrix is not symmetric positive definite.");
            }

            // Copy right hand side.
            double[][] X = B.ArrayCopy;
            int nx = B.ColumnDimension;

            // Solve L*Y = B;
            for (int k = 0; k < _n; k++) {
                for (int i = k + 1; i < _n; i++) {
                    for (int j = 0; j < nx; j++) {
                        X[i][j] -= X[k][j] * _l[i][k];
                    }
                }
                for (int j = 0; j < nx; j++) {
                    X[k][j] /= _l[k][k];
                }
            }

            // Solve L'*X = Y;
            for (int k = _n - 1; k >= 0; k--) {
                for (int j = 0; j < nx; j++) {
                    X[k][j] /= _l[k][k];
                }
                for (int i = 0; i < k; i++) {
                    for (int j = 0; j < nx; j++) {
                        X[i][j] -= X[k][j] * _l[k][i];
                    }
                }
            }
            return new GeneralMatrix(X, _n, nx);
        }

        #endregion

        // A method called when serializing this class.
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
        }
    }
}
