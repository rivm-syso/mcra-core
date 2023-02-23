namespace MCRA.Utils.NumericalRecipes {

    /// <summary>
    /// Simplex optimization
    /// TODO: ZEPS still needs the right value
    /// TODO: implement repeat in Simplex; start from minimum with random unit vectors???
    /// </summary>
    public class OptimizeSimplex {

        /// <summary>
        /// Initialize with default values MaxEvaluations=5000 and Tolerance=1.0e-6.
        /// </summary>
        public OptimizeSimplex() {
            MaxEvaluations = 5000;
            Tolerance = 1.0e-6;
        }

        /// <summary>
        /// Maximum number of function evaluations in Simplex Routine; default 5000;
        /// </summary>
        public int MaxEvaluations { get; set; }

        /// <summary>
        /// Convergence criterion in minimization routines; default 1.0e-6.
        /// </summary>
        public double Tolerance { get; set; }

        /// <summary>
        /// Number of function evaluations.
        /// </summary>
        public int Evaluations { get; private set; }

        /// <summary>
        /// Whether the minimization routine has convergence within MaxEvaluations.
        /// </summary>
        public bool Convergence { get; private set; }

        /// <summary>
        /// Multi-dimensional function.
        /// </summary>
        /// <param name="arg">Arguments of function.</param>
        /// <param name="data">Data to be passed to the function.</param>
        /// <returns>Function value.</returns>
        public delegate double Function(double[] arg, object data);

        /// <summary>
        /// Data to be passed to the Function delegate
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Finds the minimum of a multidimensional function using the Simplex method.
        /// </summary>
        /// <param name="initial">Initial estimate for the minimum.</param>
        /// <param name="minimum">Returns the minimum found by the Simplex routine.</param>
        /// <param name="function">Multi-dimensional function with double array arguments which returns a function value.</param>
        /// <param name="scale">Scale used in setting up the initial Simplex.</param>
        /// <param name="repeat">Number of times the Simplex routine must be restarted from the minimum found.</param>
        /// <returns>The minimum function value.</returns>
        /// <remarks>The initial Simplex consists of (ndim+1) points. The first point is initial and the other ndim points are formed
        /// by adding unit vectors multiplied by scale to initial. More control over the initial Simplex is given by the
        /// underlying routine <seealso cref="SimplexNR"/>. The maximum number of function evaluations can be specified by the
        /// property MaxEvaluations <seealso cref="MaxEvaluations"/> and the convergence tolerance by means of the property 
        /// Tolerance <seealso cref="Tolerance"/>.
        /// by propertie MaxN
        /// </remarks>
        public double Minimize(double[] initial, out double[] minimum, Function function, double scale, int repeat) {
            int ii, jj;
            int ndim = initial.GetLength(0);
            int ndim1 = ndim + 1;
            var simplex = new double[ndim1, ndim];
            var functionvalue = new double[ndim1];
            var fminimum =0D;
            minimum = new double[ndim];

            // Create initial Simplex and get function values at the Simplex points
            for (ii = 0; ii < ndim1; ii++) {
                for (jj = 0; jj < ndim; jj++) {
                    simplex[ii, jj] = initial[jj];
                    if (((ii - 1) == jj) && (ii > 0)) {
                        simplex[ii, jj] += scale;
                    }

                    minimum[jj] = simplex[ii, jj];
                }
                functionvalue[ii] = function(minimum, Data);
            }

            // Call Simplex routine
            Minimize(ref simplex, ref functionvalue, function);
            Evaluations += ndim1;      // Loop above
            // Mean of simplex points
            for (jj = 0; jj < ndim; jj++) {
                minimum[jj] = 0.0;
                for (ii = 0; ii < ndim1; ii++) {
                    minimum[jj] += simplex[ii, jj];
                }
                minimum[jj] /= (double)(ndim1);
            }
            Evaluations += 1;
            fminimum = function(minimum, Data);

            // Return the point with the minimal function value
            var minValue = fminimum;
            for (ii = 0; ii < ndim1; ii++) {
                if (functionvalue[ii] < minValue) {
                    minValue = functionvalue[ii];
                    for (jj = 0; jj < ndim; jj++) {
                        minimum[jj] = simplex[ii, jj];
                    }
                }
            }
            return minValue;
        }

        /// <summary>
        /// Finds the minimum of a multidimensional function using the Simplex method.
        /// </summary>
        /// <param name="p">Initial Simplex of size (ndim+1,dim) with ndim+1 points. Returns new points all within 
        /// Tolerance of a minimum function value./// </param>
        /// <param name="y">Initial function values for the ndim+1 points in the Simplex. Returns function values for 
        /// the returned points</param>
        /// <param name="function">Multi-dimensional function with double array arguments which returns a function value.</param>
        /// <remarks>Multi-dimensional minimization of function(x) where x[0...ndim-1] is a vector in ndim dimensions,
        /// by the downhill Simplex method of Nelder and Mead. The matrix p[0...ndim, 0...ndim-1] is input.
        /// Its (ndim+1) rows are ndim-dimensional vectors that are the vertices of the starting Simplex. 
        /// Also input is the vector y[0...ndim], whose components must be pre-initialized to the values of function 
        /// evaluated at the (ndim+1) vertices (rows) of p. On output, p[,] and y[] will have been reset to
        /// (ndim+1) new points all within Tolerance of a minimum function value, and Evaluations will be set 
        /// to the number of function evaluations.</remarks>
        /// <remarks>Numerical Recipes routine AMOEBA ported from C++ to C#.</remarks>
        /// <seealso cref="OptimizeSimplex"/>
        public void Minimize(ref double[,] p, ref double[] y, Function function) {
            // Proceed
            const double TINY = 1.0e-10;
            int i, ihi, ilo, inhi, j;
            double rtol, ysave, ytry;
            int npoints = p.GetLength(0), ndim = p.GetLength(1);
            double[] psum = new double[ndim];
            Evaluations = 0;
            Convergence = false;
            SimplexSum(ref p, ref psum);
            for (; ; ) {
                ilo = 0;
                if (y[0] > y[1]) { inhi = 1; ihi = 0; }
                else { inhi = 0; ihi = 1; }
                for (i = 0; i < npoints; i++) {
                    if (y[i] <= y[ilo]) {
                        ilo = i;
                    }

                    if (y[i] > y[ihi]) {
                        inhi = ihi;
                        ihi = i;
                    }
                    else if ((y[i] > y[inhi]) && (i != ihi)) {
                        inhi = i;
                    }
                }
                rtol = 2.0 * Math.Abs(y[ihi] - y[ilo]) / (Math.Abs(y[ihi]) + Math.Abs(y[ilo]) + TINY);
                if (rtol < Tolerance) {
                    Swap(ref y[0], ref y[ilo]);
                    for (i = 0; i < ndim; i++) {
                        Swap(ref p[0, i], ref p[ilo, i]);
                    }

                    Convergence = true;
                    break;
                }
                if (Evaluations >= MaxEvaluations) {
                    break;
                }
                Evaluations += 2;
                ytry = SimplexTry(ref p, ref y, ref psum, function, ihi, -1.0);
                if (ytry <= y[ilo]) {
                    ytry = SimplexTry(ref p, ref y, ref psum, function, ihi, 2.0);
                } else if (ytry >= y[inhi]) {
                    ysave = y[ihi];
                    ytry = SimplexTry(ref p, ref y, ref psum, function, ihi, 0.5);
                    if (ytry >= ysave) {
                        for (i = 0; i < npoints; i++) {
                            if (i != ilo) {
                                for (j = 0; j < ndim; j++) {
                                    p[i, j] = psum[j] = 0.5 * (p[i, j] + p[ilo, j]);
                                }

                                y[i] = function(psum, Data);
                            }
                        }
                        Evaluations += ndim;
                        SimplexSum(ref p, ref psum);
                    }
                }
                else {
                    --Evaluations;
                }
            }
        }

        private static void SimplexSum(ref double[,] p, ref double[] psum) {   // Service routine for the Simplex method.
            // Numerical Recipes routine GET_PSUM ported from C++ to C#.
            int i, j;
            int npoints = p.GetLength(0), ndim = p.GetLength(1);
            double sum;
            for (j = 0; j < ndim; j++) {
                for (sum = 0.0, i = 0; i < npoints; i++) {
                    sum += p[i, j];
                }

                psum[j] = sum;
            }
        }

        private double SimplexTry(ref double[,] p, ref double[] y, ref double[] psum, Function function, int ihi, double fac) {   // Service routine for the Simplex method.
            // Numerical Recipes routine AMOTRY ported from C++ to C#.
            int j, ndim = p.GetLength(1);
            double fac1, fac2, ytry;
            double[] ptry = new double[ndim];
            fac1 = (1.0 - fac) / ndim;
            fac2 = fac1 - fac;
            for (j = 0; j < ndim; j++) {
                ptry[j] = psum[j] * fac1 - p[ihi, j] * fac2;
            }

            ytry = function(ptry, Data);
            if (ytry < y[ihi]) {
                y[ihi] = ytry;
                for (j = 0; j < ndim; j++) {
                    psum[j] += ptry[j] - p[ihi, j];
                    p[ihi, j] = ptry[j];
                }
            }
            return ytry;
        }

        /// <summary>
        /// Swaps two arguments
        /// </summary>
        public static void Swap(ref double a, ref double b) {
            (b, a) = (a, b);
        }
    }
}
