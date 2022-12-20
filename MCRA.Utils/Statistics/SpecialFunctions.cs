using System;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Provides alternative implementations of functions in C#'s Math class.
    /// </summary>
    public static class SpecialFunctions {

        /// <summary>
        /// Returns the mean of a left truncated normal distribution.
        /// General formula: mean = mu + sd * (dnorm(lower.std)-dnorm(upper.std))/(pnorm(upper.std)-pnorm(lower.std))
        /// </summary>
        /// <param name="x"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double MeanLeftTruncatedNormal(double x, double mu, double sigma) {
            return mu + sigma * NormalDistribution.CDF(mu, sigma, x) / (1 - NormalDistribution.PDF(mu, sigma, x));
        }

        /// <summary>
        /// Ln(Gamma) function.
        /// </summary>
        /// <param name="arg">Argument.</param>
        /// <returns>Ln(Gamma) function. Returns a NaN for arg .lt. 0.</returns>
        /// <remarks>Logarithm of the Gamma function. Algorithm AS245  APPL. STATIST. (1989) VOL. 38, NO. 2.
        /// Compared with GenStat 14ed for arguments 0.001, 0.011 ... 29.991. Maximal difference 7.11E-14; maximal relative difference 5.07E-15</remarks>
        public static double LnGamma(double arg) {
            const double xlgst = 1.0e305;
            if ((arg <= 0) || (arg >= xlgst)) {
                return double.NaN;
            }
            var xx = arg;
            var alogam = 0D;
            var yy = 0D;

            // Calculation for 0 < arg < 0.5  and  0.5 <= arg < 1.5 combined
            if (xx < 1.5) {
                if (xx < 0.5) {
                    alogam = -Math.Log(xx);
                    yy = xx + 1.0;
                    if (yy == 0) {
                        return alogam;
                    }
                } else {
                    alogam = 0.0;
                    yy = xx;
                    xx = (xx - 0.5) - 0.5;
                }
                var r1 = new double[] {-2.66685511495e0, -2.44387534237e1, -2.19698958928e1,  1.11667541262e1,
                          3.13060547623e0,  6.07771387771e-1, 1.19400905721e1, 3.14690115749e1, 1.52346874070e1};
                alogam = alogam + xx * ((((r1[4] * yy + r1[3]) * yy + r1[2]) * yy + r1[1]) * yy + r1[0]) /
                   ((((yy + r1[8]) * yy + r1[7]) * yy + r1[6]) * yy + r1[5]);
                return alogam;
            }

            // Calculation for 1.5 <= arg < 4.0
            if (xx < 4.0) {
                yy = (xx - 1.0) - 1.0;
                var r2 = new double[] {-7.83359299449e1, -1.42046296688e2, 1.37519416416e2,  7.86994924154e1,
                          4.16438922228e0,  4.70668766060e1, 3.13399215894e2,  2.63505074721e2, 4.33400022514e1};
                alogam = yy * ((((r2[4] * xx + r2[3]) * xx + r2[2]) * xx + r2[1]) * xx + r2[0]) /
                        ((((xx + r2[8]) * xx + r2[7]) * xx + r2[6]) * xx + +r2[5]);
                return alogam;
            }

            // Calculation for 4.0 <= arg < 12.0
            if (xx < 12.0) {
                var r3 = new double[] {-2.12159572323e5,  2.30661510616e5, 2.74647644705e4, -4.02621119975e4,
                         -2.29660729780e3, -1.16328495004e5, -1.46025937511e5, -2.42357409629e4, -5.70691009324e2};
                alogam = ((((r3[4] * xx + r3[3]) * xx + r3[2]) * xx + r3[1]) * xx + r3[0]) /
                        ((((xx + r3[8]) * xx + r3[7]) * xx + r3[6]) * xx + +r3[5]);
                return alogam;
            }

            // Calculation for arg >= 12.0
            const double xlge = 5.10e6;
            const double alr2pi = 9.18938533204673e-1;
            var r4 = new double[] {2.79195317918525e-1, 4.917317610505968e-1, 6.92910599291889e-2,
                          3.350343815022304e0, 6.012459259764103e0};
            yy = Math.Log(xx);
            alogam = xx * (yy - 1.0) - 0.5 * yy + alr2pi;
            if (xx > xlge) {
                return alogam;
            }
            var x1 = 1.0 / xx;
            var x2 = x1 * x1;
            alogam = alogam + x1 * ((r4[2] * x2 + r4[1]) * x2 + r4[0]) / ((x2 + r4[4]) * x2 + r4[3]);
            return alogam;
        }

        /// <summary>
        /// DiGamma function.
        /// </summary>
        /// <param name="arg">Argument.</param>
        /// <returns>DiGamma function. Returns a NaN for arg .lt. 0.</returns>
        /// <remarks>First order derivative of Log(Gamma) function. Algorithm AS 103 Appl. Statist. (1976) vol.25, no.3.
        /// Compared with GenStat 14ed for arguments 0.001, 0.011 ... 29.991. Maximal difference 1.49E-10; maximal relative difference 6.99E-09.</remarks>
        public static double DiGamma(double arg) {
            const double s = 1.0e-5;
            const double c = 8.5;
            const double s3 = 8.33333333333333e-2;
            const double s4 = 8.33333333333333e-3;
            const double s5 = 3.96825396825397e-3;
            const double d1 = -0.577215664901533;  // Eulers constant
            var diGamma = 0D;
            if (arg <= 0.0) {
                return double.NaN;
            }
            if (arg <= s) {
                return d1 - 1.0 / arg;
            }
            if (arg < c) {
                do {
                    diGamma -= 1.0 / arg;
                    arg += 1.0;
                } while (arg < c);
            }
            var r = 1.0 / arg;
            diGamma += Math.Log(arg) - 0.5 * r;
            r = r * r;
            diGamma -= r * (s3 - r * (s4 - r * s5));
            return diGamma;
        }

        /// <summary>
        /// TriGamma function.
        /// </summary>
        /// <param name="arg">Argument.</param>
        /// <returns>Triamma function. Returns a NaN for arg .lt. 0.</returns>
        /// <remarks>Second order derivative of Log(Gamma) function. Algorithm AS 121 Appl. Statist. (1978) vol 27, no. 1.
        /// Compared with GenStat 14ed for arguments 0.001, 0.011 ... 29.991. Maximal difference 5.82E-09; maximal relative difference 6.20E-09</remarks>
        public static double TriGamma(double arg) {
            const double a = 1e-4;
            const double b = 5.0;
            var triGamma = 0D;
            if (arg <= 0.0) {
                return double.NaN;
            }
            var z = arg;
            if (z > a) {
                if (z < b) {
                    do {
                        triGamma += 1.0 / (z * z);
                        z += 1.0;
                    } while (z < b);
                }
            } else {
                return 1.0 / (z * z);
            }
            const double b2 = 0.166666666666667;
            const double b4 = -0.333333333333333e-1;
            const double b6 = 0.238095238095238e-1;
            const double b8 = -0.333333333333333e-1;
            var zz = 1.0 / (z * z);
            triGamma += 0.5 * zz + (1.0 + zz * (b2 + zz * (b4 + zz * (b6 + zz * b8)))) / z;
            return triGamma;
        }

        #region Gauss Hermite quadrature

        /// <summary>
        /// Calculates quadrature points and weights for two-dimensional Gauss-Hermite quadrature.
        /// </summary>
        /// <param name="nPoints">Number of quadrature points and weights.</param>
        /// <param name="prune">Pruning of quadrature points and weights. A value in the interval (0,1) will remove all points with a weight smaller than prune. 
        /// A value in the interval [1,100) will removes a percentage of points with the smallest weights (points with the same weight are kept to keep symmetry).</param>
        /// <returns>Quadrature points (in the first and second column) and weights (in the third column).</returns>
        public static double[,] GaussHermiteTwoD(int nPoints, double prune) {
            double[,] tmpXW = GaussHermite(nPoints, 0);
            int nPoints2 = nPoints * nPoints;
            double[] ghW = new double[nPoints2];
            int[] order = new int[nPoints2];
            int i, j, k = 0;
            for (i = 0; i < nPoints; i++) {
                for (j = 0; j < nPoints; j++, k++) {
                    ghW[k] = tmpXW[i, 1] * tmpXW[j, 1];
                    order[k] = k;
                }
            }
            Array.Sort(ghW, order);
            int skipPoints = pruning(ghW, prune);
            double[,] ghXXW = new double[nPoints2 - skipPoints, 3];
            // The genstat program GaussHermiteSort.gen shows that the original i and j can be derived from order[k].
            for (k = skipPoints; k < nPoints2; k++) {
                i = Convert.ToInt32(Math.Floor(Convert.ToDouble(order[k]) / nPoints));
                j = order[k] - i * nPoints;
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
        /// <param name="prune">Pruning of abscissas and weights. A value in the interval (0,1) will remove all points with a weight smaller than prune. 
        /// A value in the interval [1,100) will removes a percentage of points with the smallest weights (points with the same weight are kept to keep symmetry).</param>
        /// <returns>Quadrature points (in the first column) and weights (in the second column).</returns>
        /// <remarks>Numerical Recipes Function GAUHER ported from C++ to C#.</remarks>
        public static double[,] GaussHermite(int nPoints, double prune = 0.0) {
            double[] ghX = new double[nPoints];
            double[] ghW = new double[nPoints];
            const double epsHermite = 3.0e-14;
            const double pim4Hermite = 0.7511255444649425;
            const int maxIterHermite = 10;
            const double dZERO = 0.0, dONE = 1.0, dTWO = 2.0; ;

            int i, its, j, m;
            double p1, p2, p3, pp, z, z1, dn;
            bool returnvalue = true;
            m = (nPoints + 1) / 2;
            z = dZERO;
            pp = dZERO;
            dn = nPoints;
            for (i = 1; i <= m; i++) {
                if (i == 1) { z = Math.Sqrt(dTWO * dn + dONE) - 1.85575 * Math.Pow(dTWO * dn + dONE, -0.16667); } else if (i == 2) { z -= 1.14 * Math.Pow(dn, 0.426) / z; } else if (i == 3) { z = 1.86 * z - 0.86 * ghX[0]; } else if (i == 4) { z = 1.91 * z - 0.91 * ghX[1]; } else { z = dTWO * z - ghX[i - 3]; }
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
            for (i = 0; i < nPoints; ++i) { ghX[i] = -ghX[i]; }

            // Sort points according to weights
            Array.Sort(ghW, ghX);

            // Pruning
            int skipPoints = pruning(ghW, prune);
            // Return values in double array;
            double[,] ghXW = new double[nPoints - skipPoints, 2];
            for (i = skipPoints; i < nPoints; i++) {
                ghXW[i - skipPoints, 0] = ghX[i];
                ghXW[i - skipPoints, 1] = ghW[i];
            }
            return ghXW;
        }

        private static int pruning(double[] weights, double prune) {
            var skipPoints = 0;
            if (prune > 0.0) {
                int nPoints = weights.Length;
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
                    skipPoints = Convert.ToInt32(Math.Floor(nPoints * prune / 100D));
                    if ((skipPoints == 0) || (skipPoints == 1)) {
                        return 0;
                    }

                    int skipPoints1 = skipPoints - 1;
                    double value = weights[skipPoints1];
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

        #endregion End of Gauss Hermite quadrature

    }
}
