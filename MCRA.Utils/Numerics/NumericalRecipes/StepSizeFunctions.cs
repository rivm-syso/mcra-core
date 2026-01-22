namespace MCRA.Utils.NumericalRecipes {

    /// <summary>
    /// Added functionality
    /// </summary>
    public class StepSizeFunctions {

        /// <summary>
        /// Estimates a stepsize for the calculation of a second-order derivative given a list of function evaluations.
        /// This is done for each argument of the function.
        /// </summary>
        /// <param name="functionValues">Function values.</param>
        /// <param name="argValues">Function arguments.</param>
        /// <param name="function">Function value at arg.</param>
        /// <param name="arg">Function argument for which the second-order derivative must be calculated.</param>
        /// <param name="functionRange">Only function values F for which Abs(F - function) is smaller than functionRange are used.</param>
        /// <param name="functionChange">Change in function values which determines the stepsize.</param>
        /// <returns>Stepsize for each function argument.</returns>
        /// <remarks>Fits a linear regression Y = beta * X*X through the values Y=(functionValues - function) and X=(argValues - arg),
        /// using only Y values with an absolute value smaller than functionRange.
        /// The stepsize which induces a change in the function value of size functionChange is then given by X = Sqrt(functionChange/Abs(beta)).
        /// </remarks>
        public static double[] StepSize2(
            List<double> functionValues,
            List<double[]> argValues,
            double function,
            double[] arg,
            double functionRange,
            double functionChange
        ) {
            var length = functionValues.Count;
            var nargs = arg.Length;
            var xy = new double[nargs];
            var xx = new double[nargs];

            for (int i = 0; i < length; i++) {
                var diff = functionValues[i] - function;
                if (Math.Abs(diff) < functionRange) {
                    for (int j = 0; j < nargs; j++) {
                        var sqr = Math.Pow(argValues[i][j] - arg[j], 2);
                        xy[j] += sqr * diff;
                        xx[j] += sqr * sqr;
                    }
                }
            }
            for (int j = 0; j < nargs; j++) {
                var beta = Math.Abs(xy[j] / xx[j]);
                xy[j] = Math.Sqrt(functionChange / beta);
            }
            return xy;
        }

        /// <summary>
        /// Back transformation of usual exposure for the power transformation by means of Gauss-Hermite integration
        /// </summary>
        /// <param name="mean">Value to back transform</param>
        /// <param name="sigma2">Within Individuals Variance</param>
        /// <param name="invpower">Inverse of Power Transformation</param>
        /// <param name="ghXW">Gauss-Hermite weights and points</param>
        /// <param name="nGH">Number of Gauss-Hermite points.</param>
        /// <returns></returns>
        public static double GaussHermiteBackTransformation(
            double mean,
            double sigma2,
            double invpower,
            double[,] ghXW,
            int nGH
        ) {
            var invSqrtPi = 1.0 / Math.Sqrt(Math.PI);
            var factor = Math.Sqrt(2.0 * sigma2);
            var returnvalue = 0d;
            var integerPower = Convert.ToDouble(Math.Floor(invpower)) == invpower;
            for (int ii = 0; ii < nGH; ++ii) {
                var arg = mean + factor * ghXW[ii, 0];
                if ((integerPower) || (arg > 0.0)) {
                    returnvalue += ghXW[ii, 1] * Math.Pow(arg, invpower);
                }
            }
            return returnvalue * invSqrtPi;
        }
    }
}
