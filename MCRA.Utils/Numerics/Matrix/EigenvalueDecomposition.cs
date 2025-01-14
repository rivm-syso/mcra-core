using System.Runtime.Serialization;

namespace MCRA.Utils {

    /// <summary>Eigenvalues and eigenvectors of a real matrix.
    /// If A is symmetric, then A = V*D*V' where the eigenvalue matrix D is
    /// diagonal and the eigenvector matrix V is orthogonal.
    /// I.e. A = V.Multiply(D.Multiply(V.Transpose())) and
    /// V.Multiply(V.Transpose()) equals the identity matrix.
    /// If A is not symmetric, then the eigenvalue matrix D is block diagonal
    /// with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    /// lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda].  The
    /// columns of V represent the eigenvectors in the sense that A*V = V*D,
    /// i.e. A.Multiply(V) equals V.Multiply(D).  The matrix V may be badly
    /// conditioned, or even singular, so the validity of the equation
    /// A = V*D*Inverse(V) depends upon V.cond().
    /// </summary>
    [Serializable]
    public class EigenvalueDecomposition : ISerializable {

        #region	 Class variables

        /// <summary>Row and column dimension (square matrix).
        /// @serial matrix dimension.
        /// </summary>
        private readonly int _n;

        /// <summary>Symmetry flag.
        /// @serial internal symmetry flag.
        /// </summary>
        private readonly bool _isSymmetric;

        /// <summary>Arrays for internal storage of eigenvalues.
        /// @serial internal storage of eigenvalues.
        /// </summary>
        private readonly double[] _d, _e;

        /// <summary>Array for internal storage of eigenvectors.
        /// @serial internal storage of eigenvectors.
        /// </summary>
        private readonly double[][] _V;

        /// <summary>Array for internal storage of nonsymmetric Hessenberg form.
        /// @serial internal storage of nonsymmetric Hessenberg form.
        /// </summary>
        private readonly double[][] _H;

        /// <summary>Working storage for nonsymmetric algorithm.
        /// @serial working storage for nonsymmetric algorithm.
        /// </summary>
        private readonly double[] _ort;

        #endregion //  Class variables

        #region Private Methods

        // Symmetric Householder reduction to tridiagonal form.

        private void tred2() {
            //  This is derived from the Algol procedures tred2 by
            //  Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            //  Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            //  Fortran subroutine in EISPACK.

            for (int j = 0; j < _n; j++) {
                _d[j] = _V[_n - 1][j];
            }

            // Householder reduction to tridiagonal form.
            for (int i = _n - 1; i > 0; i--) {
                // Scale to avoid under/overflow.

                double scale = 0.0;
                double h = 0.0;
                for (int k = 0; k < i; k++) {
                    scale = scale + Math.Abs(_d[k]);
                }
                if (scale == 0.0) {
                    _e[i] = _d[i - 1];
                    for (int j = 0; j < i; j++) {
                        _d[j] = _V[i - 1][j];
                        _V[i][j] = 0.0;
                        _V[j][i] = 0.0;
                    }
                } else {
                    // Generate Householder vector.

                    for (int k = 0; k < i; k++) {
                        _d[k] /= scale;
                        h += _d[k] * _d[k];
                    }
                    double f = _d[i - 1];
                    double g = Math.Sqrt(h);
                    if (f > 0) {
                        g = -g;
                    }
                    _e[i] = scale * g;
                    h = h - f * g;
                    _d[i - 1] = f - g;
                    for (int j = 0; j < i; j++) {
                        _e[j] = 0.0;
                    }

                    // Apply similarity transformation to remaining columns.
                    for (int j = 0; j < i; j++) {
                        f = _d[j];
                        _V[j][i] = f;
                        g = _e[j] + _V[j][j] * f;
                        for (int k = j + 1; k <= i - 1; k++) {
                            g += _V[k][j] * _d[k];
                            _e[k] += _V[k][j] * f;
                        }
                        _e[j] = g;
                    }
                    f = 0.0;
                    for (int j = 0; j < i; j++) {
                        _e[j] /= h;
                        f += _e[j] * _d[j];
                    }
                    double hh = f / (h + h);
                    for (int j = 0; j < i; j++) {
                        _e[j] -= hh * _d[j];
                    }
                    for (int j = 0; j < i; j++) {
                        f = _d[j];
                        g = _e[j];
                        for (int k = j; k <= i - 1; k++) {
                            _V[k][j] -= (f * _e[k] + g * _d[k]);
                        }
                        _d[j] = _V[i - 1][j];
                        _V[i][j] = 0.0;
                    }
                }
                _d[i] = h;
            }

            // Accumulate transformations.
            for (int i = 0; i < _n - 1; i++) {
                _V[_n - 1][i] = _V[i][i];
                _V[i][i] = 1.0;
                double h = _d[i + 1];
                if (h != 0.0) {
                    for (int k = 0; k <= i; k++) {
                        _d[k] = _V[k][i + 1] / h;
                    }
                    for (int j = 0; j <= i; j++) {
                        double g = 0.0;
                        for (int k = 0; k <= i; k++) {
                            g += _V[k][i + 1] * _V[k][j];
                        }
                        for (int k = 0; k <= i; k++) {
                            _V[k][j] -= g * _d[k];
                        }
                    }
                }
                for (int k = 0; k <= i; k++) {
                    _V[k][i + 1] = 0.0;
                }
            }
            for (int j = 0; j < _n; j++) {
                _d[j] = _V[_n - 1][j];
                _V[_n - 1][j] = 0.0;
            }
            _V[_n - 1][_n - 1] = 1.0;
            _e[0] = 0.0;
        }

        // Symmetric tridiagonal QL algorithm.
        private void tql2() {
            //  This is derived from the Algol procedures tql2, by
            //  Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            //  Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            //  Fortran subroutine in EISPACK.
            for (int i = 1; i < _n; i++) {
                _e[i - 1] = _e[i];
            }
            _e[_n - 1] = 0.0;

            double f = 0.0;
            double tst1 = 0.0;
            double eps = Math.Pow(2.0, -52.0);
            for (int l = 0; l < _n; l++) {
                // Find small subdiagonal element

                tst1 = Math.Max(tst1, Math.Abs(_d[l]) + Math.Abs(_e[l]));
                int m = l;
                while (m < _n) {
                    if (Math.Abs(_e[m]) <= eps * tst1) {
                        break;
                    }
                    m++;
                }

                // If m == l, d[l] is an eigenvalue,
                // otherwise, iterate.

                if (m > l) {
                    int iter = 0;
                    do {
                        iter = iter + 1; // (Could check iteration count here.)

                        // Compute implicit shift

                        double g = _d[l];
                        double p = (_d[l + 1] - g) / (2.0 * _e[l]);
                        double r = Maths.Hypot(p, 1.0);
                        if (p < 0) {
                            r = -r;
                        }
                        _d[l] = _e[l] / (p + r);
                        _d[l + 1] = _e[l] * (p + r);
                        double dl1 = _d[l + 1];
                        double h = g - _d[l];
                        for (int i = l + 2; i < _n; i++) {
                            _d[i] -= h;
                        }
                        f = f + h;

                        // Implicit QL transformation.

                        p = _d[m];
                        double c = 1.0;
                        double c2 = c;
                        double c3 = c;
                        double el1 = _e[l + 1];
                        double s = 0.0;
                        double s2 = 0.0;
                        for (int i = m - 1; i >= l; i--) {
                            c3 = c2;
                            c2 = c;
                            s2 = s;
                            g = c * _e[i];
                            h = c * p;
                            r = Maths.Hypot(p, _e[i]);
                            _e[i + 1] = s * r;
                            s = _e[i] / r;
                            c = p / r;
                            p = c * _d[i] - s * g;
                            _d[i + 1] = h + s * (c * g + s * _d[i]);

                            // Accumulate transformation.

                            for (int k = 0; k < _n; k++) {
                                h = _V[k][i + 1];
                                _V[k][i + 1] = s * _V[k][i] + c * h;
                                _V[k][i] = c * _V[k][i] - s * h;
                            }
                        }
                        p = (-s) * s2 * c3 * el1 * _e[l] / dl1;
                        _e[l] = s * p;
                        _d[l] = c * p;

                        // Check for convergence.
                    }
                    while (Math.Abs(_e[l]) > eps * tst1);
                }
                _d[l] = _d[l] + f;
                _e[l] = 0.0;
            }

            // Sort eigenvalues and corresponding vectors.

            for (int i = 0; i < _n - 1; i++) {
                int k = i;
                double p = _d[i];
                for (int j = i + 1; j < _n; j++) {
                    if (_d[j] < p) {
                        k = j;
                        p = _d[j];
                    }
                }
                if (k != i) {
                    _d[k] = _d[i];
                    _d[i] = p;
                    for (int j = 0; j < _n; j++) {
                        p = _V[j][i];
                        _V[j][i] = _V[j][k];
                        _V[j][k] = p;
                    }
                }
            }
        }

        // Nonsymmetric reduction to Hessenberg form.

        private void orthes() {
            //  This is derived from the Algol procedures orthes and ortran,
            //  by Martin and Wilkinson, Handbook for Auto. Comp.,
            //  Vol.ii-Linear Algebra, and the corresponding
            //  Fortran subroutines in EISPACK.

            int low = 0;
            int high = _n - 1;

            for (int m = low + 1; m <= high - 1; m++) {

                // Scale column.

                double scale = 0.0;
                for (int i = m; i <= high; i++) {
                    scale = scale + Math.Abs(_H[i][m - 1]);
                }
                if (scale != 0.0) {

                    // Compute Householder transformation.

                    double h = 0.0;
                    for (int i = high; i >= m; i--) {
                        _ort[i] = _H[i][m - 1] / scale;
                        h += _ort[i] * _ort[i];
                    }
                    double g = Math.Sqrt(h);
                    if (_ort[m] > 0) {
                        g = -g;
                    }
                    h = h - _ort[m] * g;
                    _ort[m] = _ort[m] - g;

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)

                    for (int j = m; j < _n; j++) {
                        double f = 0.0;
                        for (int i = high; i >= m; i--) {
                            f += _ort[i] * _H[i][j];
                        }
                        f = f / h;
                        for (int i = m; i <= high; i++) {
                            _H[i][j] -= f * _ort[i];
                        }
                    }

                    for (int i = 0; i <= high; i++) {
                        double f = 0.0;
                        for (int j = high; j >= m; j--) {
                            f += _ort[j] * _H[i][j];
                        }
                        f = f / h;
                        for (int j = m; j <= high; j++) {
                            _H[i][j] -= f * _ort[j];
                        }
                    }
                    _ort[m] = scale * _ort[m];
                    _H[m][m - 1] = scale * g;
                }
            }

            // Accumulate transformations (Algol's ortran).

            for (int i = 0; i < _n; i++) {
                for (int j = 0; j < _n; j++) {
                    _V[i][j] = (i == j ? 1.0 : 0.0);
                }
            }

            for (int m = high - 1; m >= low + 1; m--) {
                if (_H[m][m - 1] != 0.0) {
                    for (int i = m + 1; i <= high; i++) {
                        _ort[i] = _H[i][m - 1];
                    }
                    for (int j = m; j <= high; j++) {
                        double g = 0.0;
                        for (int i = m; i <= high; i++) {
                            g += _ort[i] * _V[i][j];
                        }
                        // Double division avoids possible underflow
                        g = (g / _ort[m]) / _H[m][m - 1];
                        for (int i = m; i <= high; i++) {
                            _V[i][j] += g * _ort[i];
                        }
                    }
                }
            }
        }


        // Complex scalar division.

        [NonSerialized()]
        private double cdivr, cdivi;

        private void cdiv(double xr, double xi, double yr, double yi) {
            double r, d;
            if (Math.Abs(yr) > Math.Abs(yi)) {
                r = yi / yr;
                d = yr + r * yi;
                cdivr = (xr + r * xi) / d;
                cdivi = (xi - r * xr) / d;
            } else {
                r = yr / yi;
                d = yi + r * yr;
                cdivr = (r * xr + xi) / d;
                cdivi = (r * xi - xr) / d;
            }
        }


        // Nonsymmetric reduction from Hessenberg to real Schur form.

        private void hqr2() {
            //  This is derived from the Algol procedure hqr2,
            //  by Martin and Wilkinson, Handbook for Auto. Comp.,
            //  Vol.ii-Linear Algebra, and the corresponding
            //  Fortran subroutine in EISPACK.

            // Initialize

            int nn = this._n;
            int n = nn - 1;
            int low = 0;
            int high = nn - 1;
            double eps = Math.Pow(2.0, -52.0);
            double exshift = 0.0;
            double p = 0, q = 0, r = 0, s = 0, z = 0, t, w, x, y;

            // Store roots isolated by balanc and compute matrix norm

            double norm = 0.0;
            for (int i = 0; i < nn; i++) {
                if (i < low || i > high) {
                    _d[i] = _H[i][i];
                    _e[i] = 0.0;
                }
                for (int j = Math.Max(i - 1, 0); j < nn; j++) {
                    norm = norm + Math.Abs(_H[i][j]);
                }
            }

            // Outer loop over eigenvalue index

            int iter = 0;
            while (n >= low) {

                // Look for single small sub-diagonal element

                int l = n;
                while (l > low) {
                    s = Math.Abs(_H[l - 1][l - 1]) + Math.Abs(_H[l][l]);
                    if (s == 0.0) {
                        s = norm;
                    }
                    if (Math.Abs(_H[l][l - 1]) < eps * s) {
                        break;
                    }
                    l--;
                }

                // Check for convergence
                // One root found

                if (l == n) {
                    _H[n][n] = _H[n][n] + exshift;
                    _d[n] = _H[n][n];
                    _e[n] = 0.0;
                    n--;
                    iter = 0;

                    // Two roots found
                } else if (l == n - 1) {
                    w = _H[n][n - 1] * _H[n - 1][n];
                    p = (_H[n - 1][n - 1] - _H[n][n]) / 2.0;
                    q = p * p + w;
                    z = Math.Sqrt(Math.Abs(q));
                    _H[n][n] = _H[n][n] + exshift;
                    _H[n - 1][n - 1] = _H[n - 1][n - 1] + exshift;
                    x = _H[n][n];

                    // Real pair

                    if (q >= 0) {
                        if (p >= 0) {
                            z = p + z;
                        } else {
                            z = p - z;
                        }
                        _d[n - 1] = x + z;
                        _d[n] = _d[n - 1];
                        if (z != 0.0) {
                            _d[n] = x - w / z;
                        }
                        _e[n - 1] = 0.0;
                        _e[n] = 0.0;
                        x = _H[n][n - 1];
                        s = Math.Abs(x) + Math.Abs(z);
                        p = x / s;
                        q = z / s;
                        r = Math.Sqrt(p * p + q * q);
                        p = p / r;
                        q = q / r;

                        // Row modification

                        for (int j = n - 1; j < nn; j++) {
                            z = _H[n - 1][j];
                            _H[n - 1][j] = q * z + p * _H[n][j];
                            _H[n][j] = q * _H[n][j] - p * z;
                        }

                        // Column modification

                        for (int i = 0; i <= n; i++) {
                            z = _H[i][n - 1];
                            _H[i][n - 1] = q * z + p * _H[i][n];
                            _H[i][n] = q * _H[i][n] - p * z;
                        }

                        // Accumulate transformations

                        for (int i = low; i <= high; i++) {
                            z = _V[i][n - 1];
                            _V[i][n - 1] = q * z + p * _V[i][n];
                            _V[i][n] = q * _V[i][n] - p * z;
                        }

                        // Complex pair
                    } else {
                        _d[n - 1] = x + p;
                        _d[n] = x + p;
                        _e[n - 1] = z;
                        _e[n] = -z;
                    }
                    n = n - 2;
                    iter = 0;

                    // No convergence yet
                } else {

                    // Form shift

                    x = _H[n][n];
                    y = 0.0;
                    w = 0.0;
                    if (l < n) {
                        y = _H[n - 1][n - 1];
                        w = _H[n][n - 1] * _H[n - 1][n];
                    }

                    // Wilkinson's original ad hoc shift

                    if (iter == 10) {
                        exshift += x;
                        for (int i = low; i <= n; i++) {
                            _H[i][i] -= x;
                        }
                        s = Math.Abs(_H[n][n - 1]) + Math.Abs(_H[n - 1][n - 2]);
                        x = y = 0.75 * s;
                        w = (-0.4375) * s * s;
                    }

                    // MATLAB's new ad hoc shift

                    if (iter == 30) {
                        s = (y - x) / 2.0;
                        s = s * s + w;
                        if (s > 0) {
                            s = Math.Sqrt(s);
                            if (y < x) {
                                s = -s;
                            }
                            s = x - w / ((y - x) / 2.0 + s);
                            for (int i = low; i <= n; i++) {
                                _H[i][i] -= s;
                            }
                            exshift += s;
                            x = y = w = 0.964;
                        }
                    }

                    iter = iter + 1; // (Could check iteration count here.)

                    // Look for two consecutive small sub-diagonal elements

                    int m = n - 2;
                    while (m >= l) {
                        z = _H[m][m];
                        r = x - z;
                        s = y - z;
                        p = (r * s - w) / _H[m + 1][m] + _H[m][m + 1];
                        q = _H[m + 1][m + 1] - z - r - s;
                        r = _H[m + 2][m + 1];
                        s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                        p = p / s;
                        q = q / s;
                        r = r / s;
                        if (m == l) {
                            break;
                        }
                        if (Math.Abs(_H[m][m - 1]) * (Math.Abs(q) + Math.Abs(r)) < eps * (Math.Abs(p) * (Math.Abs(_H[m - 1][m - 1]) + Math.Abs(z) + Math.Abs(_H[m + 1][m + 1])))) {
                            break;
                        }
                        m--;
                    }

                    for (int i = m + 2; i <= n; i++) {
                        _H[i][i - 2] = 0.0;
                        if (i > m + 2) {
                            _H[i][i - 3] = 0.0;
                        }
                    }

                    // Double QR step involving rows l:n and columns m:n

                    for (int k = m; k <= n - 1; k++) {
                        bool notlast = (k != n - 1);
                        if (k != m) {
                            p = _H[k][k - 1];
                            q = _H[k + 1][k - 1];
                            r = (notlast ? _H[k + 2][k - 1] : 0.0);
                            x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            if (x != 0.0) {
                                p = p / x;
                                q = q / x;
                                r = r / x;
                            }
                        }
                        if (x == 0.0) {
                            break;
                        }
                        s = Math.Sqrt(p * p + q * q + r * r);
                        if (p < 0) {
                            s = -s;
                        }
                        if (s != 0) {
                            if (k != m) {
                                _H[k][k - 1] = (-s) * x;
                            } else if (l != m) {
                                _H[k][k - 1] = -_H[k][k - 1];
                            }
                            p = p + s;
                            x = p / s;
                            y = q / s;
                            z = r / s;
                            q = q / p;
                            r = r / p;

                            // Row modification

                            for (int j = k; j < nn; j++) {
                                p = _H[k][j] + q * _H[k + 1][j];
                                if (notlast) {
                                    p = p + r * _H[k + 2][j];
                                    _H[k + 2][j] = _H[k + 2][j] - p * z;
                                }
                                _H[k][j] = _H[k][j] - p * x;
                                _H[k + 1][j] = _H[k + 1][j] - p * y;
                            }

                            // Column modification

                            for (int i = 0; i <= Math.Min(n, k + 3); i++) {
                                p = x * _H[i][k] + y * _H[i][k + 1];
                                if (notlast) {
                                    p = p + z * _H[i][k + 2];
                                    _H[i][k + 2] = _H[i][k + 2] - p * r;
                                }
                                _H[i][k] = _H[i][k] - p;
                                _H[i][k + 1] = _H[i][k + 1] - p * q;
                            }

                            // Accumulate transformations

                            for (int i = low; i <= high; i++) {
                                p = x * _V[i][k] + y * _V[i][k + 1];
                                if (notlast) {
                                    p = p + z * _V[i][k + 2];
                                    _V[i][k + 2] = _V[i][k + 2] - p * r;
                                }
                                _V[i][k] = _V[i][k] - p;
                                _V[i][k + 1] = _V[i][k + 1] - p * q;
                            }
                        } // (s != 0)
                    } // k loop
                } // check convergence
            } // while (n >= low)

            // Backsubstitute to find vectors of upper triangular form

            if (norm == 0.0) {
                return;
            }

            for (n = nn - 1; n >= 0; n--) {
                p = _d[n];
                q = _e[n];

                // Real vector

                if (q == 0) {
                    int l = n;
                    _H[n][n] = 1.0;
                    for (int i = n - 1; i >= 0; i--) {
                        w = _H[i][i] - p;
                        r = 0.0;
                        for (int j = l; j <= n; j++) {
                            r = r + _H[i][j] * _H[j][n];
                        }
                        if (_e[i] < 0.0) {
                            z = w;
                            s = r;
                        } else {
                            l = i;
                            if (_e[i] == 0.0) {
                                if (w != 0.0) {
                                    _H[i][n] = (-r) / w;
                                } else {
                                    _H[i][n] = (-r) / (eps * norm);
                                }

                                // Solve real equations
                            } else {
                                x = _H[i][i + 1];
                                y = _H[i + 1][i];
                                q = (_d[i] - p) * (_d[i] - p) + _e[i] * _e[i];
                                t = (x * s - z * r) / q;
                                _H[i][n] = t;
                                if (Math.Abs(x) > Math.Abs(z)) {
                                    _H[i + 1][n] = (-r - w * t) / x;
                                } else {
                                    _H[i + 1][n] = (-s - y * t) / z;
                                }
                            }

                            // Overflow control

                            t = Math.Abs(_H[i][n]);
                            if ((eps * t) * t > 1) {
                                for (int j = i; j <= n; j++) {
                                    _H[j][n] = _H[j][n] / t;
                                }
                            }
                        }
                    }

                    // Complex vector
                } else if (q < 0) {
                    int l = n - 1;

                    // Last vector component imaginary so matrix is triangular

                    if (Math.Abs(_H[n][n - 1]) > Math.Abs(_H[n - 1][n])) {
                        _H[n - 1][n - 1] = q / _H[n][n - 1];
                        _H[n - 1][n] = (-(_H[n][n] - p)) / _H[n][n - 1];
                    } else {
                        cdiv(0.0, -_H[n - 1][n], _H[n - 1][n - 1] - p, q);
                        _H[n - 1][n - 1] = cdivr;
                        _H[n - 1][n] = cdivi;
                    }
                    _H[n][n - 1] = 0.0;
                    _H[n][n] = 1.0;
                    for (int i = n - 2; i >= 0; i--) {
                        double ra, sa, vr, vi;
                        ra = 0.0;
                        sa = 0.0;
                        for (int j = l; j <= n; j++) {
                            ra = ra + _H[i][j] * _H[j][n - 1];
                            sa = sa + _H[i][j] * _H[j][n];
                        }
                        w = _H[i][i] - p;

                        if (_e[i] < 0.0) {
                            z = w;
                            r = ra;
                            s = sa;
                        } else {
                            l = i;
                            if (_e[i] == 0) {
                                cdiv(-ra, -sa, w, q);
                                _H[i][n - 1] = cdivr;
                                _H[i][n] = cdivi;
                            } else {

                                // Solve complex equations

                                x = _H[i][i + 1];
                                y = _H[i + 1][i];
                                vr = (_d[i] - p) * (_d[i] - p) + _e[i] * _e[i] - q * q;
                                vi = (_d[i] - p) * 2.0 * q;
                                if (vr == 0.0 && vi == 0.0) {
                                    vr = eps * norm * (Math.Abs(w) + Math.Abs(q) + Math.Abs(x) + Math.Abs(y) + Math.Abs(z));
                                }
                                cdiv(x * r - z * ra + q * sa, x * s - z * sa - q * ra, vr, vi);
                                _H[i][n - 1] = cdivr;
                                _H[i][n] = cdivi;
                                if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q))) {
                                    _H[i + 1][n - 1] = (-ra - w * _H[i][n - 1] + q * _H[i][n]) / x;
                                    _H[i + 1][n] = (-sa - w * _H[i][n] - q * _H[i][n - 1]) / x;
                                } else {
                                    cdiv(-r - y * _H[i][n - 1], -s - y * _H[i][n], z, q);
                                    _H[i + 1][n - 1] = cdivr;
                                    _H[i + 1][n] = cdivi;
                                }
                            }

                            // Overflow control

                            t = Math.Max(Math.Abs(_H[i][n - 1]), Math.Abs(_H[i][n]));
                            if ((eps * t) * t > 1) {
                                for (int j = i; j <= n; j++) {
                                    _H[j][n - 1] = _H[j][n - 1] / t;
                                    _H[j][n] = _H[j][n] / t;
                                }
                            }
                        }
                    }
                }
            }

            // Vectors of isolated roots

            for (int i = 0; i < nn; i++) {
                if (i < low || i > high) {
                    for (int j = i; j < nn; j++) {
                        _V[i][j] = _H[i][j];
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix

            for (int j = nn - 1; j >= low; j--) {
                for (int i = low; i <= high; i++) {
                    z = 0.0;
                    for (int k = low; k <= Math.Min(j, high); k++) {
                        z = z + _V[i][k] * _H[k][j];
                    }
                    _V[i][j] = z;
                }
            }
        }

        #endregion //  Private Methods

        #region Constructor

        /// <summary>Check for symmetry, then construct the eigenvalue decomposition</summary>
        /// <param name="Arg">   Square matrix
        /// </param>
        /// <returns>     Structure to access D and V.
        /// </returns>
        public EigenvalueDecomposition(GeneralMatrix Arg) {
            double[][] A = Arg.Array;
            _n = Arg.ColumnDimension;
            _V = new double[_n][];
            for (int i = 0; i < _n; i++) {
                _V[i] = new double[_n];
            }
            _d = new double[_n];
            _e = new double[_n];

            _isSymmetric = true;
            for (int j = 0; (j < _n) && _isSymmetric; j++) {
                for (int i = 0; (i < _n) && _isSymmetric; i++) {
                    _isSymmetric = (A[i][j] == A[j][i]);
                }
            }

            if (_isSymmetric) {
                for (int i = 0; i < _n; i++) {
                    for (int j = 0; j < _n; j++) {
                        _V[i][j] = A[i][j];
                    }
                }

                // Tridiagonalize.
                tred2();

                // Diagonalize.
                tql2();
            } else {
                _H = new double[_n][];
                for (int i2 = 0; i2 < _n; i2++) {
                    _H[i2] = new double[_n];
                }
                _ort = new double[_n];

                for (int j = 0; j < _n; j++) {
                    for (int i = 0; i < _n; i++) {
                        _H[i][j] = A[i][j];
                    }
                }

                // Reduce to Hessenberg form.
                orthes();

                // Reduce Hessenberg to real Schur form.
                hqr2();
            }
        }

        #endregion //  Constructor

        #region Public Properties

        /// <summary>Return the real parts of the eigenvalues</summary>
        /// <returns>     real(diag(D))
        /// </returns>
        virtual public double[] RealEigenvalues {
            get {
                return _d;
            }
        }

        /// <summary>Return the imaginary parts of the eigenvalues</summary>
        /// <returns>imag(diag(D))</returns>
        virtual public double[] ImagEigenvalues {
            get {
                return _e;
            }
        }

        /// <summary>Return the block diagonal eigenvalue matrix</summary>
        /// <returns>D</returns>
        virtual public GeneralMatrix D {
            get {
                GeneralMatrix X = new GeneralMatrix(_n, _n);
                double[][] D = X.Array;
                for (int i = 0; i < _n; i++) {
                    for (int j = 0; j < _n; j++) {
                        D[i][j] = 0.0;
                    }
                    D[i][i] = _d[i];
                    if (_e[i] > 0) {
                        D[i][i + 1] = _e[i];
                    } else if (_e[i] < 0) {
                        D[i][i - 1] = _e[i];
                    }
                }
                return X;
            }
        }
        #endregion //  Public Properties

        #region Public Methods

        /// <summary>Return the eigenvector matrix</summary>
        /// <returns>V</returns>
        public virtual GeneralMatrix GetV() {
            return new GeneralMatrix(_V, _n, _n);
        }

        #endregion //  Public Methods

        // A method called when serializing this class.
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
        }
    }
}