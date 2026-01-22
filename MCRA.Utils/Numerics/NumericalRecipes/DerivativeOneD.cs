namespace MCRA.Utils.NumericalRecipes {
    /// <summary>
    /// Calculates the first- and second-order derivative of a one-dimensional function.
    /// </summary>
    public class DerivativeOneD {

        /// <summary>
        /// Number of function evaluations
        /// </summary>
        public int Evaluations { get; set; }

        /// <summary>
        /// Stepsize (as a multiplication factor) used in Method DerivateStepSize; default value is 0.05.
        /// </summary>
        /// <seealso cref="StepSizeDefault"/>
        public double StepSizeDefault { get; set; }= 0.05;

        /// <summary>
        /// The absolute maximum number of extrapolations.
        /// </summary>
        protected const int MaxMaxCycles = 10;

        /// <summary>
        /// Maximum number of extrapolations in Ridders method for calculating derivatives. 
        /// Default 2 and maximum value is 10; non-positive values are set to the maximum.
        /// </summary>
        public int MaxCycles = 10;

        /// <summary>
        /// One-dimensional function.
        /// </summary>
        /// <param name="arg">Argument of function.</param>
        /// <param name="data">Data to be passed to the function.</param>
        /// <returns>Function value.</returns>
        public delegate double Function(double arg, object data);
        /// <summary>
        /// Data to be passed to the Function delegate
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Returns the (default) stepsize for the calculation of derivatives of a One-dimensional function
        /// </summary>
        /// <param name="x">Parameter for which a stepsize must be calculated</param>
        /// <returns>StepSizeDefault times the argument x, with a minimum value of StepSizeDefault.</returns>
        /// <seealso cref="StepSizeDefault"/>
        public double StepSizeFixed(double x) {
            double step = Math.Abs(StepSizeDefault * x);
            if (step < StepSizeDefault) {
                step = StepSizeDefault;
            }

            return step;
        }


        /// <summary>
        /// Approximates the derivative of a One-dimensional function using Ridders' method of polynomial extrapolation.
        /// </summary>
        /// <param name="function">One-dimensional function for which the derivative is required.</param>
        /// <param name="x">Value for which the derivative must be approximated.</param>
        /// <param name="step">Initial stepsize; should be set to an increment in x over which the function changes considerably.</param>
        /// <param name="error">Returns an estimate of the error in the derivative.</param>
        /// <returns>The derivative at point x.</returns>
        /// <remarks>Number of function evaluations is at most 2*ntimes. No error estimate is returned for ntimes=1.</remarks>
        /// <remarks>Numerical Recipes routine DFRIDR ported from C++ to C#.</remarks>
        public double Derivative(Function function, double x, double step, out double error) {
            Evaluations = 0;

            int NTAB;
            if (MaxCycles <= 0) { NTAB = MaxMaxCycles; }
            else { NTAB = Math.Min(MaxCycles, MaxMaxCycles); }
            double[,] a = new double[NTAB, NTAB];

            const double CON = 1.4, CON2 = CON * CON;
            const double BIG = 1.0e30;
            const double SAFE = 2.0;
            int i, j;
            double errt, fac, hh, ans = double.NaN;
            if (step == 0.0) { throw new Exception("step must be nonzero in Derivative."); }
            hh = step;
            Evaluations += 2;
            a[0, 0] = (function(x + hh, Data) - function(x - hh, Data)) / (2.0 * hh);
            ans = a[0, 0];
            error = BIG;
            for (i = 1; i < NTAB; i++) {
                hh /= CON;
                Evaluations += 2;
                a[0, i] = (function(x + hh, Data) - function(x - hh, Data)) / (2.0 * hh);
                fac = CON2;
                for (j = 1; j <= i; j++) {
                    a[j, i] = (a[j - 1, i] * fac - a[j - 1, i - 1]) / (fac - 1.0);
                    fac = CON2 * fac;
                    errt = Math.Max(Math.Abs(a[j, i] - a[j - 1, i]), Math.Abs(a[j, i] - a[j - 1, i - 1]));
                    if (errt <= error) {
                        error = errt;
                        ans = a[j, i];
                    }
                }
                if (Math.Abs(a[i, i] - a[i - 1, i - 1]) >= SAFE * error) {
                    break;
                }
            }
            return ans;
        }

        /// <summary>
        /// Approximates the 2nd derivative of a One-dimensional function using Ridders' method of polynomial extrapolation.
        /// </summary>
        /// <param name="function">One-dimensional function for which the 2nd derivative is required.</param>
        /// <param name="x">Value for which the derivative must be approximated.</param>
        /// <param name="fx">Function value at x; if set to NaN the function value is calculated (and returned).</param>
        /// <param name="step">Initial stepsize; should be set to an increment in x over which the function changes considerably.</param>
        /// <param name="error">Returns an estimate of the error in the 2nd derivative.</param>
        /// <returns>The 2nd derivative at point x.</returns>
        /// <remarks>Number of function evaluations is at most 1 + 2*ntimes. No error estimate is returned for ntimes=1.</remarks>
        /// <remarks>Numerical Recipes routine DFRIDR ported from C++ to C#.</remarks>
        public double Derivative2(Function function, double x, ref double fx, double step, out double error) {
            Evaluations = 0;

            int NTAB;
            if (MaxCycles <= 0) { NTAB = MaxMaxCycles; }
            else { NTAB = Math.Min(MaxCycles, MaxMaxCycles); }
            double[,] a = new double[NTAB, NTAB];

            const double CON = 1.4, CON2 = CON * CON;
            const double BIG = 1.0e30;
            double SAFE = 2.0;
            int i, j;
            double errt, fac, hh, ans = double.NaN;
            if (step == 0.0) { throw new Exception("h must be nonzero in Derivative2."); }
            hh = step;
            if (double.IsNaN(fx)) {
                Evaluations += 1;
                fx = function(x, Data);
            }
            Evaluations += 2;
            a[0, 0] = (function(x + hh, Data) - 2.0 * fx + function(x - hh, Data)) / (hh * hh);
            ans = a[0, 0];
            error = BIG;
            for (i = 1; i < NTAB; i++) {
                hh /= CON;
                Evaluations += 2;
                a[0, i] = (function(x + hh, Data) - 2.0 * fx + function(x - hh, Data)) / (hh * hh);
                fac = CON2;
                for (j = 1; j <= i; j++) {
                    a[j, i] = (a[j - 1, i] * fac - a[j - 1, i - 1]) / (fac - 1.0);
                    fac = CON2 * fac;
                    errt = Math.Max(Math.Abs(a[j, i] - a[j - 1, i]), Math.Abs(a[j, i] - a[j - 1, i - 1]));
                    if (errt <= error) {
                        error = errt;
                        ans = a[j, i];
                    }
                }
                if (Math.Abs(a[i, i] - a[i - 1, i - 1]) >= SAFE * error) {
                    break;
                }
            }
            return ans;
        }
    }
}
