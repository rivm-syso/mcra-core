namespace MCRA.Utils.NumericalRecipes {
    /// <summary>
    /// Calculates the gradient and hessian of a multi-dimensional function.
    /// </summary>
    public class DerivativeMultiD {

        public DerivativeMultiD() {
            StepSizeDefault = 0.05;
            MaxCycles = 10;
        }

        /// <summary>
        /// Number of function evaluations
        /// </summary>
        public int Evaluations { get; private set; }

        /// <summary>
        /// Stepsize (as a multiplication factor) used in Method StepSize; default value is 0.05.
        /// </summary>
        public double StepSizeDefault { get; set; }

        /// <summary>
        /// The absolute maximum number of extrapolations.
        /// </summary>
        protected const int MaxMaxCycles = 10;
        /// <summary>
        /// Maximum number of extrapolations in Ridders method for calculating derivatives. 
        /// Default and maximum value is 10; non-positive values are set to the maximum.
        /// </summary>
        public int MaxCycles { get; set; } = 2;

        /// <summary>
        /// Multi-dimensional function.
        /// </summary>
        /// <param name="arg">Argument of function.</param>
        /// <param name="data">Data to be passed to the function.</param>
        /// <returns>Function value.</returns>
        public delegate double Function(double[] arg, object data);

        /// <summary>
        /// Data to be passed to the function to optimize
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Returns (default) stepsizes for the calculation of derivatives of a Multi-dimensional function
        /// <param name="x">Parameters for which a stepsize must be calculated</param>
        /// <returns>StepSizeDefault times the argument x[], with a minimum value of StepSizeDefault.</returns>
        /// <seealso cref="StepSizeDefault"/>
        public double[] StepSize(double[] x) {
            var ndim = x.GetLength(0);
            var step = new double[ndim];
            for (int ii = 0; ii < ndim; ii++) {
                step[ii] = Math.Abs(StepSizeDefault * x[ii]);
                if (step[ii] < StepSizeDefault) {
                    step[ii] = StepSizeDefault;
                }
            }
            return step;
        }

        /// <summary>
        /// Approximates the Gradient of a Multi-dimensional function using Ridders' method of polynomial extrapolation.
        /// </summary>
        /// <param name="function">Multi-dimensional function for which the derivative is required.</param>
        /// <param name="x">Values for which the derivative must be approximated.</param>
        /// <param name="step">Initial stepsize; should be set to an increment in x over which the function changes considerably.</param>
        /// <param name="error">Returns an estimate of the error in the derivative.</param>
        /// <returns>The derivative at point x.</returns>
        /// <remarks>Numerical Recipes routine DFRIDR ported from C++ to C#.</remarks>
        public double[] Gradient(Function function, double[] x, double[] step, out double error) {
            Evaluations = 0;
            var NTAB = MaxCycles <= 0 ? MaxMaxCycles : Math.Min(MaxCycles, MaxMaxCycles);
            double[,] a = new double[NTAB, NTAB];

            const double CON = 1.4, CON2 = CON * CON;
            const double BIG = 1.0e30;
            const double SAFE = 2.0;
            int i, j;
            double err, errt, fac, hh, ans;
            int ndim = x.GetLength(0);
            double[] xmin = new double[ndim], xplus = new double[ndim], deriv = new double[ndim];
            error = double.NegativeInfinity;
            // Loop over dimensions
            for (int idim = 0; idim < ndim; idim++) {
                if (step[idim] == 0.0) {
                    throw new Exception("step[" + idim.ToString() + "] must be nonzero in Gradient.");
                }
                for (i = 0; i < ndim; i++) {
                    xmin[i] = x[i]; xplus[i] = x[i];
                }
                hh = step[idim];
                xmin[idim] = x[idim] - hh;
                xplus[idim] = x[idim] + hh;
                Evaluations += 2;
                a[0, 0] = (function(xplus, Data) - function(xmin, Data)) / (2.0 * hh);
                err = BIG;
                ans = a[0, 0];
                for (i = 1; i < NTAB; i++) {
                    hh /= CON;
                    xmin[idim] = x[idim] - hh;
                    xplus[idim] = x[idim] + hh;
                    Evaluations += 2;
                    a[0, i] = (function(xplus, Data) - function(xmin, Data)) / (2.0 * hh);
                    fac = CON2;
                    for (j = 1; j <= i; j++) {
                        a[j, i] = (a[j - 1, i] * fac - a[j - 1, i - 1]) / (fac - 1.0);
                        fac = CON2 * fac;
                        errt = Math.Max(Math.Abs(a[j, i] - a[j - 1, i]), Math.Abs(a[j, i] - a[j - 1, i - 1]));
                        if (errt <= err) {
                            err = errt;
                            ans = a[j, i];
                        }
                    }
                    if (Math.Abs(a[i, i] - a[i - 1, i - 1]) >= SAFE * err) {
                        break;
                    }
                }
                deriv[idim] = ans;
                if (err > error) {
                    error = err;
                }
            }
            return deriv;
        }

        /// <summary>
        /// Approximates the Hessian of a Multi-dimensional function using Ridders' method of polynomial extrapolation.
        /// </summary>
        /// <param name="function">Multi-dimensional function for which the Hessian is required.</param>
        /// <param name="x">Values for which the Hessian must be approximated.</param>
        /// <param name="step">Initial stepsize; should be set to an increment in x over which the function changes considerably.</param>
        /// <param name="error">Returns an estimate of the error in the derivative.</param>
        /// <returns>The Hessian at point x.</returns>
        /// <remarks>Numerical Recipes routine DFRIDR ported from C++ to C#.</remarks>
        public double[,] Hessian(
            Function function,
            double[] x,
            double[] step,
            out double error
        ) {
            Evaluations = 0;

            int NTAB;
            if (MaxCycles <= 0) { NTAB = MaxMaxCycles; } else { NTAB = Math.Min(MaxCycles, MaxMaxCycles); }
            double[,] a = new double[NTAB, NTAB];

            const double CON = 1.4, CON2 = CON * CON;
            const double BIG = 1.0e30;
            const double SAFE = 2.0;
            int i, j;
            double err, errt, fac, hhi, hhj, ans, fpp, fpm, fmp, fmm;
            int ndim = x.GetLength(0);
            double[] xtmp = new double[ndim];
            double[,] deriv = new double[ndim, ndim];
            double[] hsave = new double[ndim];
            error = double.NegativeInfinity;

            Evaluations += 1;
            var fx = function(x, Data);

            for (int idim = 0; idim < ndim; idim++) {
                if (step[idim] == 0.0) {
                    throw new Exception($"step[{idim}] must be nonzero in Hessian.");
                }
                for (i = 0; i < ndim; i++) {
                    xtmp[i] = x[i];
                }
                hhi = step[idim];
                Evaluations += 2;
                xtmp[idim] = x[idim] - hhi;
                fmm = function(xtmp, Data);
                xtmp[idim] = x[idim] + hhi;
                fpp = function(xtmp, Data);
                a[0, 0] = (fpp - 2.0 * fx + fmm) / (hhi * hhi);
                err = BIG;
                ans = a[0, 0];
                for (i = 1; i < NTAB; i++) {
                    hhi /= CON;
                    Evaluations += 2;
                    xtmp[idim] = x[idim] - hhi; fmm = function(xtmp, Data);
                    xtmp[idim] = x[idim] + hhi; fpp = function(xtmp, Data);
                    a[0, i] = (fpp - 2.0 * fx + fmm) / (hhi * hhi);
                    fac = CON2;
                    for (j = 1; j <= i; j++) {
                        a[j, i] = (a[j - 1, i] * fac - a[j - 1, i - 1]) / (fac - 1.0);
                        fac = CON2 * fac;
                        errt = Math.Max(Math.Abs(a[j, i] - a[j - 1, i]), Math.Abs(a[j, i] - a[j - 1, i - 1]));
                        if (errt <= err) {
                            err = errt;
                            ans = a[j, i];
                        }
                    }
                    if (Math.Abs(a[i, i] - a[i - 1, i - 1]) >= SAFE * err) {
                        hsave[idim] = hhi;
                        break;
                    }
                }
                deriv[idim, idim] = ans;
                if (err > error) {
                    error = err;
                }
            }
            // Loop over dimensions and approximate non-diagonal elements
            for (int idim = 0; idim < ndim; idim++) {
                for (int jdim = 0; jdim < idim; jdim++) {
                    for (i = 0; i < ndim; i++) { xtmp[i] = x[i]; }
                    hhi = step[idim];
                    hhj = step[jdim];
                    Evaluations += 4;
                    xtmp[idim] = x[idim] - hhi; xtmp[jdim] = x[jdim] - hhj; fmm = function(xtmp, Data);
                    xtmp[idim] = x[idim] - hhi; xtmp[jdim] = x[jdim] + hhj; fmp = function(xtmp, Data);
                    xtmp[idim] = x[idim] + hhi; xtmp[jdim] = x[jdim] - hhj; fpm = function(xtmp, Data);
                    xtmp[idim] = x[idim] + hhi; xtmp[jdim] = x[jdim] + hhj; fpp = function(xtmp, Data);
                    a[0, 0] = (fpp - fpm - fmp + fmm) / (4.0 * hhi * hhj);
                    err = BIG;
                    ans = a[0, 0];
                    for (i = 1; i < NTAB; i++) {
                        hhi /= CON; hhj /= CON;
                        Evaluations += 4;
                        xtmp[idim] = x[idim] - hhi; xtmp[jdim] = x[jdim] - hhj; fmm = function(xtmp, Data);
                        xtmp[idim] = x[idim] - hhi; xtmp[jdim] = x[jdim] + hhj; fmp = function(xtmp, Data);
                        xtmp[idim] = x[idim] + hhi; xtmp[jdim] = x[jdim] - hhj; fpm = function(xtmp, Data);
                        xtmp[idim] = x[idim] + hhi; xtmp[jdim] = x[jdim] + hhj; fpp = function(xtmp, Data);
                        a[0, i] = (fpp - fpm - fmp + fmm) / (4.0 * hhi * hhj);
                        fac = CON2;
                        for (j = 1; j <= i; j++) {
                            a[j, i] = (a[j - 1, i] * fac - a[j - 1, i - 1]) / (fac - 1.0);
                            fac = CON2 * fac;
                            errt = Math.Max(Math.Abs(a[j, i] - a[j - 1, i]), Math.Abs(a[j, i] - a[j - 1, i - 1]));
                            if (errt <= err) {
                                err = errt;
                                ans = a[j, i];
                            }
                        }
                        if (Math.Abs(a[i, i] - a[i - 1, i - 1]) >= SAFE * err) {
                            break;
                        }
                    }
                    deriv[idim, jdim] = ans;
                    deriv[jdim, idim] = ans;
                    if (err > error) {
                        error = err;
                    }
                }
            }

            return deriv;
        }
    }
}
