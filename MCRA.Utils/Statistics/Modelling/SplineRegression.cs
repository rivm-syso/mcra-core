using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Statistics.Modelling {

    public static class SplineRegression {

        /// <summary>
        /// Calculates integrated I-splines and performs a monotone regression.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="wt"></param>
        /// <param name="knots"></param>
        public static List<double> MonotoneRegressionSpline(
            List<double> x,
            List<double> y,
            List<double> wt,
            double[] knots
        ) {
            var splineBase = calculateSpline(x, knots);
            return rNonNegative(splineBase, y, wt);
        }

        /// <summary>
        /// Calculates I-spline (summing M-splines).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="zKnots"></param>
        private static double[,] calculateSpline(List<double> x, double[] zKnots) {
            var order = 3;
            var nx = x.Count;
            var upper = x.Max() + 1e-15 * x.Max();
            var lower = x.Min();
            var nKnots = zKnots.Length;
            var minKnots = zKnots[0];
            var maxKnots = zKnots[nKnots - 1];
            var k = 1 * order + 0 * nKnots + 1;
            var kmin = 1 * order + 0 * nKnots + 0;
            var kplus = 1 * order + 0 * nKnots + 2;
            var nt = 2 * order + 1 * nKnots + 2;
            var ntmin = 2 * order + 1 * nKnots + 1;
            var nbase = 1 * order + 1 * nKnots + 1;
            var nbasemin = 1 * order + 1 * nKnots + 0;
            var nbaseplus = 1 * order + 1 * nKnots + 2;

            var t = new double[nt];
            for (int i = 0; i < k; i++) {
                t[i] = lower;
                t[i + nKnots + k] = upper;
            }
            for (int i = 0; i < nKnots; i++) {
                t[i + k] = zKnots[i];
            }

            var ik = new int[nbase, kplus];
            var tel = new int[nbase];
            for (int i = 0; i < nbase; i++) {
                tel[i] = i + 1;
                for (int j = 0; j < kplus; j++) {
                    ik[i, j] = i + j + 1;
                }
            }

            // initialize m
            var m = new double[nx, ntmin];
            for (int i = 0; i < nx; i++) {
                for (int j = 0; j < nbase; j++) {
                    if ((x[i] >= t[j] && x[i] < t[j + 1]) && t[j + 1] - t[j] != 0) {
                        m[i, j] = 1.0 / (t[j + 1] - t[j]);
                    }
                }
            }
            //M-splines are calculated using a recurrence relation
            var iimin = 0D;
            var ii = 1D;
            int iiplus = 2;
            for (int kk = 0; kk < kmin; kk++) {
                iimin++;
                ii++;
                for (int i = 0; i < nx; i++) {
                    for (int j = 0; j < nbase; j++) {
                        if ((t[j + iiplus] - t[j]) != 0) {
                            m[i, j] = (ii / iimin) * ((x[i] - t[j]) * m[i, j] + (t[j + iiplus] - x[i]) * m[i, j + 1]) / (t[j + iiplus] - t[j]);
                        }
                    }
                }
                iiplus++;
            }
            //I-splines are obtained by summing M-splines
            var splineBase = new double[nx, nbasemin];
            var dummy = new double[nx];
            var jj = k - 1;
            var jjplus = k;

            for (int i = 0; i < nKnots + 1; i++) {
                ii = 0;
                iiplus = 1;
                var condition = new int[nx];
                for (int p = 0; p < nx; p++) {
                    if (x[p] >= t[jj] && x[p] < t[jjplus]) {
                        condition[p] = 1;
                    }
                }
                for (int q = 0; q < nbasemin; q++) {
                    for (int p = 0; p < nx; p++) {
                        double dd = 0;
                        if (condition[p] == 1) {
                            if (iiplus > jj) {
                                splineBase[p, q] = 0;
                            } else if (iiplus < (jj - order + 1)) {
                                splineBase[p, q] = 1;
                            } else {
                                for (int r = 0; r < nbase; r++) {
                                    if (tel[r] >= iiplus && tel[r] <= jj) {
                                        int mm = tel[r];
                                        int mmk = mm + k;
                                        dd = dd + (t[mmk] - t[mm]) * m[p, mm] / k;
                                        splineBase[p, q] = dd;
                                    }
                                }
                            }
                        }
                    }
                    ii++;
                    iiplus++;
                }
                jj++;
                jjplus++;
            }
            return splineBase;
        }

        /// <summary>
        /// For monotone increasing fit.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="wt"></param>
        private static List<double> rNonNegative(double[,] x, List<double> y, List<double> wt) {
            var n = x.GetLength(0);
            var m = x.GetLength(1);
            var covar = new double[m];
            var mean = new double[m];
            var free = (double[,])x.Clone();

            //Predictors are scaled because this increases numerical precision in the
            //calculation of the Kuhn-Tucker conditions. Store copies of predictors.
            for (int i = 0; i < m; i++) {
                var temp = new double[n];
                for (int j = 0; j < n; j++) {
                    temp[j] = x[j, i];
                }

                covar[i] = Math.Sqrt(temp.Variance());
                mean[i] = temp.Average();
            }

            var meany = y.Average();
            var sey = Math.Sqrt(y.Variance());
            var scaley = new double[n];

            //Scaled variates are used in calculation of Kuhn-Tucker
            for (int i = 0; i < n; i++) {
                scaley[i] = (y[i] - meany) / sey;
                for (int j = 0; j < m; j++) {
                    if (covar[j] != 0) {
                        free[i, j] = (free[i, j] - mean[j]) / covar[j];
                    } else {
                        free[i, j] = free[i, j] - mean[j];
                    }
                }
            }

            //double noParam = 0;
            var exclude = new int[m];
            var beta = new double[m];

            //initial fit
            var designX0 = createDesignMatrix(free, exclude);
            var mrResult = MultipleLinearRegressionCalculator.Compute(designX0, y, wt);
            var fit = mrResult.FittedValues.ToArray();

            //TODO check out SPLINE, a print might be useful
            for (int index = 0; index < 100; index++) {
                int critBeta = 0;
                int crit = 0;
                int ix = 0;

                for (int j = 0; j < m; j++) {
                    if (beta[j] < 0) {
                        critBeta++;
                    }
                }

                if (critBeta == 0) {
                    for (int j = 0; j < m; j++) {
                        if (exclude[j] == 1) {
                            crit++;
                        }
                        if (crit == m) {
                            break;
                        }
                    }

                    var sumFree = new double[m];
                    var ncol = new int[m];
                    var max0 = -1e10;
                    for (int i = 0; i < n; i++) {
                        for (int j = 0; j < m; j++) {
                            if (exclude[j] == 0) {
                                sumFree[j] += free[i, j] * wt[i] * (scaley[i] - (fit[i] - meany) / sey);
                            }
                        }
                    }
                    for (int j = 0; j < m; j++) {
                        if (exclude[j] == 0) {
                            covar[j] = sumFree[j];
                            if (covar[j] > max0) {
                                max0 = covar[j];
                                ix = j;
                            }
                        }
                    }

                    if (max0 < 0) {
                        break;
                    }
                    exclude[ix] = 1;
                } else {
                    double min0 = 1e10;
                    for (int j = 0; j < m; j++) {
                        if (beta[j] < min0) {
                            min0 = beta[j];
                            ix = j;
                        }
                    }
                    exclude[ix] = 0;
                }

                var designX = createDesignMatrix(free, exclude);
                var _mrResult = MultipleLinearRegressionCalculator.Compute(designX, y, wt);
                fit = _mrResult.FittedValues.ToArray();
                var estim = _mrResult.RegressionCoefficients.ToArray();
                var counter = 0;
                for (int j = 0; j < m; j++) {
                    beta[j] = 0;
                    if (exclude[j] == 1) {
                        counter++;
                        beta[j] = estim[counter];
                    }
                }
            }

            var finalX = createDesignMatrix(x, exclude);
            return MultipleLinearRegressionCalculator.Compute(finalX, y, wt).FittedValues;
        }

        /// <summary>
        /// Constructs a design matrix based on a covariable, levels are sorted and the first 
        /// level is the reference level in regression (dummy is one).
        /// </summary>
        /// <param name="v"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private static double[,] createDesignMatrix(double[,] v, int[] col) {
            var m = col.Length;
            var n = v.GetLength(0);
            var sumCol = 0;
            for (int i = 0; i < m; i++) {
                if (col[i] != 0) {
                    sumCol++;
                }
            }
            var x = new double[n, sumCol + 1];

            for (int i = 0; i < n; i++) {
                x[i, 0] = 1;
            }
            if (m == 0) {
                return x;
            }

            for (int i = 0; i < n; i++) {
                var index = 0;
                for (int j = 0; j < m; j++) {
                    if (col[j] == 1) {
                        index++;
                        x[i, index] = v[i, j];
                    }
                }
            }
            return x;
        }
    }
}
