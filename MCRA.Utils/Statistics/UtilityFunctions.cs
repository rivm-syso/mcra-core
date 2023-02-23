using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Utils {

    /// <summary>
    /// Utility functions
    /// </summary>
    public static class UtilityFunctions {
        private const double ROOT2PI = 2.506628274631001e0;
        private const double EXPARGUPPER = 20.0;
        private const double EXPARGLOWER = -20.0;
        private const double LOGARGUPPER = 4.851651954097903E+08; // Exp(EXPARGUPPER)
        private const double LOGARGLOWER = 2.061153622438558E-09; // Exp(EXPARGLOWER)

        public const int GaussHermitePoints = 32;

        /// <summary>
        /// Probability density of Normal distribution.
        /// </summary>
        /// <param name="X">Abscissa</param>
        /// <param name="Mean">Mean of Normal distribution.</param>
        /// <param name="Variance">Variance of Normal distribution.</param>
        /// <param name="SqrtVariance">Square Root of Variance of Normal distribution.</param>
        /// <returns>Probability density of Normal distribution.</returns>
        public static double PRNormal(double X, double Mean, double Variance, double SqrtVariance) {
            var dTmp = (X - Mean);
            return ExpBound(-0.5 * dTmp * dTmp / Variance) / (ROOT2PI * SqrtVariance);
        }

        /// <summary>
        /// Probability density of standard Normal distribution.
        /// </summary>
        /// <param name="X">Abscissa.</param>
        /// <returns>Probability density of standard Normal distribution.</returns>
        public static double PRNormal(double X) {
            return ExpBound(-0.5 * X * X) / ROOT2PI;
        }

        /// <summary>
        /// Logit function (bounded).
        /// </summary>
        /// <param name="arg">Argument.</param>
        /// <returns>Logit. Argument bounded between 9.999999979388463e-01 and 2.061153618190204e-09. Result bounded between -20 and 20.</returns>
        public static double Logit(double arg) {
            return LogBound(arg / (1D - arg));
        }

        /// <summary>
        /// Inverse of Logit function (bounded).
        /// </summary>
        /// <param name="arg">Argument.</param>
        /// <returns>Inverse Logit. Argument bounded between -20 and 20. Result bounded between 9.999999979388463e-01 and 2.061153618190204e-09.</returns>
        /// // Inverse of Logit function (bounded).
        public static double ILogit(double arg) {
            return 1D / (1D + ExpBound(-arg));
        }

        /// <summary>
        /// Exponential function (bounded).
        /// </summary>
        /// <param name="arg">Argument</param>
        /// <returns>Exponential. Argument bounded between -20 and 20. Result bounded between 2.061153622438558E-09 and 4.851651954097903E+08.</returns>
        public static double ExpBound(double arg) {
            return Math.Exp(Bound(arg, EXPARGLOWER, EXPARGUPPER));
        }

        /// <summary>
        /// Natural Logarithm (bounded).
        /// </summary>
        /// <param name="arg">Argument.</param>
        /// <returns>Natural logarithm. Argument bounded between 2.061153622438558E-09 and 4.851651954097903E+08. Result bounded between -20 and 20.</returns>
        public static double LogBound(double arg) {
            return Math.Log(arg.Bound(LOGARGLOWER, LOGARGUPPER));
        }

        /// <summary>
        /// Bounds argument between lower and upper limits.
        /// </summary>
        /// <param name="arg">Argument.</param>
        /// <param name="lowerLimit">Lower limit.</param>
        /// <param name="upperLimit">Upper Limit</param>
        /// <returns>Bounded value.</returns>
        public static double Bound(this double arg, double lowerLimit, double upperLimit) {
            if (arg < lowerLimit) {
                return lowerLimit;
            } else if (arg > upperLimit) {
                return upperLimit;
            } else {
                return arg;
            }
        }

        /// <summary>
        /// BoxCox power transformation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static double BoxCox(double x, double power) {
            if (x > 0) {
                return (Math.Pow(x, power) - 1) / power;
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Inverse BoxCox power transformation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static double InverseBoxCox(double x, double power) {
            var args = power * x + 1;
            if (args > 0) {
                return Math.Pow(args, 1 / power);
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Back-transformation for Model Based Amount for BoxCox transformation.
        /// </summary>
        /// <param name="mean">Individual effect (including the mean) on the transformed scale.</param>
        /// <param name="varWithin">Within individual Variance.</param>
        /// <param name="power">Power Transformation.</param>
        /// <param name="ghXW">Gauss-Hermite weights and points.</param>
        /// <returns>Back-transformed amount.</returns>
        public static double BackTransformAmountBoxCox(double mean, double varWithin, double power, double[,] ghXW) {
            var nGH = ghXW.GetLength(0);
            var invSqrtPi = 1.0 / Math.Sqrt(Math.PI);
            var factor = Math.Sqrt(2.0 * varWithin);
            var returnvalue = 0D;
            for (int ii = 0; ii < nGH; ++ii) {
                var arg = mean + factor * ghXW[ii, 0];
                returnvalue += ghXW[ii, 1] * InverseBoxCox(arg, power);
            }
            return returnvalue * invSqrtPi;
        }

        #region Gauss Hermite quadrature

        /// <summary>
        /// Calculates quadrature points and weights for two-dimensional Gauss-Hermite quadrature.
        /// </summary>
        /// <param name="nPoints">Number of quadrature points and weights.</param>
        /// <returns>Quadrature points (in the first and second column) and weights (in the third column).</returns>
        public static double[,] GaussHermiteTwoD(int nPoints) {
            return GaussHermiteTwoD(nPoints, -1.0);
        }

        /// <summary>
        /// Calculates quadrature points and weights for two-dimensional Gauss-Hermite quadrature.
        /// </summary>
        /// <param name="nPoints">Number of quadrature points and weights.</param>
        /// <param name="prune">Pruning of quadrature points and weights. A value in the interval (0,1) will remove all points with a weight smaller than prune. 
        /// A value in the interval [1,100) will removes a percentage of points with the smallest weights (points with the same weight are kept to keep symmetry).</param>
        /// <returns>Quadrature points (in the first and second column) and weights (in the third column).</returns>
        public static double[,] GaussHermiteTwoD(int nPoints, double prune) {
            var tmpXW = GaussHermite(nPoints, 0);
            var nPoints2 = nPoints * nPoints;
            var ghW = new double[nPoints2];
            var order = new int[nPoints2];
            var k = 0;
            for (int i = 0; i < nPoints; i++) {
                for (int j = 0; j < nPoints; j++, k++) {
                    ghW[k] = tmpXW[i, 1] * tmpXW[j, 1];
                    order[k] = k;
                }
            }
            Array.Sort(ghW, order);
            var skipPoints = pruning(ghW, prune);
            var ghXXW = new double[nPoints2 - skipPoints, 3];
            // The genstat program GaussHermiteSort.gen shows that the original i and j can be derived from order[k].
            for (k = skipPoints; k < nPoints2; k++) {
                var i = BMath.Floor(Convert.ToDouble(order[k]) / nPoints);
                var j = order[k] - i * nPoints;
                ghXXW[k - skipPoints, 0] = tmpXW[i, 0];
                ghXXW[k - skipPoints, 1] = tmpXW[j, 0];
                ghXXW[k - skipPoints, 2] = ghW[k];
            }
            return ghXXW;
        }

        /// <summary>
        /// Calculates quadrature points and weights for Gauss-Hermite quadrature.
        /// </summary>
        /// <param name="nPoints">Number of quadrature points and weights.</param>
        /// <returns>Quadrature points (in the first column) and weights (in the second column).</returns>
        /// <remarks>Numerical Recipes Function GAUHER ported from C++ to C#.</remarks>
        public static double[,] GaussHermite(int nPoints) {
            return GaussHermite(nPoints, -1.0);
        }

        /// <summary>
        /// Calculates quadrature points and weights for Gauss-Hermite quadrature.
        /// </summary>
        /// <param name="nPoints">Number of quadrature points and weights.</param>
        /// <param name="prune">Pruning of abscissas and weights. A value in the interval (0,1) will remove all points with a weight smaller than prune. 
        /// A value in the interval [1,100) will removes a percentage of points with the smallest weights (points with the same weight are kept to keep symmetry).</param>
        /// <returns>Quadrature points (in the first column) and weights (in the second column).</returns>
        /// <remarks>Numerical Recipes Function GAUHER ported from C++ to C#.</remarks>
        public static double[,] GaussHermite(int nPoints, double prune = 0.0) {
            var ghX = new double[nPoints];
            var ghW = new double[nPoints];
            const double epsHermite = 3.0e-14;
            const double pim4Hermite = 0.7511255444649425;
            const int maxIterHermite = 10;
            const double dZERO = 0.0, dONE = 1.0, dTWO = 2.0; ;

            int i, its, j, m;
            double p1, p2, p3, pp, z, z1, dn;
            var returnvalue = true;
            m = (nPoints + 1) / 2;
            z = dZERO;
            pp = dZERO;
            dn = nPoints;
            for (i = 1; i <= m; i++) {
                if (i == 1) {
                    z = Math.Sqrt(dTWO * dn + dONE) - 1.85575 * Math.Pow(dTWO * dn + dONE, -0.16667);
                } else if (i == 2) {
                    z -= 1.14 * Math.Pow(dn, 0.426) / z;
                } else if (i == 3) {
                    z = 1.86 * z - 0.86 * ghX[0];
                } else if (i == 4) {
                    z = 1.91 * z - 0.91 * ghX[1];
                } else {
                    z = dTWO * z - ghX[i - 3];
                }
                for (its = 1; its <= maxIterHermite; its++) {
                    p1 = pim4Hermite;
                    p2 = dZERO;
                    for (j = 1; j <= nPoints; j++) {
                        p3 = p2;
                        p2 = p1;
                        p1 = z * Math.Sqrt(dTWO / j) * p2 - Math.Sqrt(((double)(j - 1)) / j) * p3;
                    }
                    pp = Math.Sqrt(dTWO * dn) * p2;
                    z1 = z;
                    z = z1 - p1 / pp;
                    if (Math.Abs(z - z1) <= epsHermite) {
                        break;
                    }
                }
                if (its > maxIterHermite) {
                    returnvalue = false;
                }
                ghX[i - 1] = z;
                ghX[nPoints - i] = -z;
                ghW[i - 1] = dTWO / (pp * pp);
                ghW[nPoints - i] = ghW[i - 1];
            }
            if (returnvalue == false) {
                throw new Exception("Too many iterations in GaussHermite");
            }
            for (i = 0; i < nPoints; ++i) {
                ghX[i] = -ghX[i];
            }

            // Sort points according to weights
            Array.Sort(ghW, ghX);

            // Pruning
            var skipPoints = pruning(ghW, prune);
            // Return values in double array;
            var ghXW = new double[nPoints - skipPoints, 2];
            for (i = skipPoints; i < nPoints; i++) {
                ghXW[i - skipPoints, 0] = ghX[i];
                ghXW[i - skipPoints, 1] = ghW[i];
            }
            return ghXW;
        }

        private static int pruning(double[] weights, double prune) {
            var skipPoints = 0;
            if (prune > 0.0) {
                var nPoints = weights.Length;
                if (prune < 1.0) {
                    // Prune all points with a weight smaller than prune
                    for (int i = 0; i < nPoints; i++) {
                        if (weights[i] < prune) {
                            skipPoints++;
                        } else {
                            break;
                        }
                    }
                } else if (prune < 100) {
                    // Prune a percentages and round the number of points down to keep symmetry (i.e. all points with the same weight are kept).
                    skipPoints = BMath.Floor(nPoints * prune / 100.0);
                    if ((skipPoints == 0) || (skipPoints == 1)) {
                        return 0;
                    }
                    var skipPoints1 = skipPoints - 1;
                    var value = weights[skipPoints1];
                    if (value >= weights[skipPoints]) {
                        skipPoints--;
                        for (int i = 1; i <= skipPoints1; i++) {
                            if (weights[skipPoints1 - i] == value) {
                                skipPoints--;
                            } else {
                                break;
                            }
                        }
                    }
                    if (skipPoints < 0) {
                        skipPoints = 0;
                    }
                }
                if (skipPoints >= nPoints) {
                    throw new Exception("No quadrature points left after pruning in GaussHermite");
                }
            }
            return skipPoints;
        }

        /// <summary>
        /// Shows that pruning with percentages always retains the quadrature points with the same weight to keep symmetry.
        /// Outputs the weights and the number of points for each weight;
        /// </summary>
        public static void GaussHermiteTestPrune() {
            for (int k = 4; k < 10; k++) {
                var nRows = 0;
                for (int j = 0; j < 76; j++) {
                    var gh = GaussHermiteTwoD(k, Convert.ToDouble(j));
                    var nCurrent = gh.GetLength(0);
                    if (nCurrent != nRows) {
                        System.Diagnostics.Debug.WriteLine("\nNumber of Points: " + k.ToString() + "  Percentage: " + j.ToString());
                        var count = 1;
                        for (int i = 0; i < nCurrent; i++) {
                            if (i > 0) {
                                if (gh[i, 2] == gh[i - 1, 2]) {
                                    count++;
                                } else {
                                    System.Diagnostics.Debug.WriteLine($"{gh[i - 1, 2]:F2}".PadLeft(20) + count.ToString().PadLeft(10));
                                    count = 1;
                                }
                            }
                        }
                        System.Diagnostics.Debug.WriteLine($"{gh[nCurrent - 1, 2]:F2}".PadLeft(20) + count.ToString().PadLeft(10));
                    }
                    nRows = nCurrent;
                }
            }
        }

        #endregion End of Gauss Hermite quadrature

        /// <summary>
        /// Binomial Coefficient C(n,k). Number of combinations of k objects taken from a set of size n
        /// </summary>
        /// <param name="n">Size of set.</param>
        /// <param name="k">Number of objects.</param>
        /// <returns>Binomial Coefficient C(n,k)</returns>
        public static double BinomialCoefficient(int n, int k) {
            var bico = 1D;
            double dn = n;
            int upper;
            if (2 * k <= n) {
                upper = k;
            } else {
                upper = n - k;
            }
            for (int i = 0; i < upper; i++) {
                bico *= (dn - i) / (i + 1.0);
            }
            return bico;
        }

        /// <summary>
        /// Linear inter- and extrapolation. The results are interpolated zhat-values, or values on the original-scale.
        /// </summary>
        /// <param name="x">z-values: Blom-scores</param>
        /// <param name="gY">gz-values: fitted values of t</param>
        /// <param name="y">t-values: transformed exposure amounts</param>
        /// <returns></returns>
        public static List<double> LinearInterpolate(List<double> x, List<double> gY, List<double> y) {
            var n = x.Count;
            var m = y.Count;
            var xHat = new double[m];
            // for extrapolation to the left-side
            var copyx = new double[n];
            var copygY = new double[n];
            x.CopyTo(copyx);
            gY.CopyTo(copygY);

            for (int i = 0; i < x.Count - 1; i++) {
                if (x[i + 1] < x[i]) {
                    //throw new Exception("ISUF integration was not succesfull: increase gridsize (current value = " + Globals.ISUF_Grid + ")<br>");
                }
            }

            if (y[0] < gY[0]) {
                var zStart = new List<double> { copyx[0], copyx[1] };
                var gZStart = new List<double> { copygY[0], copygY[1] };
                try {
                    var slrResult = SimpleLinearRegressionCalculator.Compute(zStart, gZStart);
                    var rr = (y[0] - slrResult.Constant) / slrResult.Coefficient;
                    copyx[0] = rr - 0.001 * Math.Abs(rr);
                    copygY[0] = slrResult.Constant + slrResult.Coefficient * copyx[0];
                } catch (Exception) {
                    copyx[0] = 0.0;
                    copygY[0] = gZStart[0];
                }
            }

            //for extrapolation to the right-side
            if (y[m - 1] > copygY[n - 1]) {
                var zStart = new List<double> { copyx[n - 2], copyx[n - 1] };
                var gZStart = new List<double> { copygY[n - 2], copygY[n - 1] };
                var slrResult = SimpleLinearRegressionCalculator.Compute(zStart, gZStart);
                var rr = (y[m - 1] - slrResult.Constant) / slrResult.Coefficient;
                copyx[n - 1] = rr + 0.001 * Math.Abs(rr);
                copygY[n - 1] = slrResult.Constant + slrResult.Coefficient * copyx[n - 1];
            }

            int j;
            for (int i = 0; i < m; i++) {
                for (j = 0; j < n; j++) {
                    if (y[i] <= copygY[j]) {
                        break;
                    }
                }
                if (j == n) {
                    j = j - 1;
                }
                if (j == 0) {
                    j += 1;
                }
                var rangeZ = copyx[j] - copyx[j - 1];
                var rangeGZ = copygY[j] - copygY[j - 1];
                xHat[i] = copyx[j - 1] + rangeZ / rangeGZ * (y[i] - copygY[j - 1]);
                if (double.IsNaN(xHat[i])) {
                    xHat[i] = 0;
                }
            }
            return xHat.ToList();
        }
    }
}
