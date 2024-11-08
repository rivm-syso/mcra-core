using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    ///
    /// </summary>
    public static class StatisticalTests {

        /// <summary>
        /// Returns loglikelihood ratio tests. Note that for forward selection a different approach
        /// to determine the number of degrees of freedom of the polynomial is followed than for
        /// backward selection
        /// </summary>
        /// <param name="modelResults"></param>
        /// <returns></returns>
        public static LikelihoodRatioTestResults GetLikelihoodRatioTest(this List<ModelResult> modelResults) {
            if (modelResults.Count > 1) {
                var dfPol = new List<int>();
                var logLik0 = modelResults.Take(modelResults.Count - 1).Select(c => c._2LogLikelihood);
                var logLik1 = modelResults.Skip(1).Select(c => c._2LogLikelihood);
                var deltaDf0 = modelResults.Take(modelResults.Count - 1).Select(c => c.DegreesOfFreedom);
                var deltaDf1 = modelResults.Skip(1).Select(c => c.DegreesOfFreedom);
                var deltaChi = logLik0.Zip(logLik1, (x, y) => Math.Abs(x - y)).ToList();
                var deltaDf = deltaDf0.Zip(deltaDf1, (x, y) => Math.Abs(x - y)).ToList();
                var pValue = new List<double>();
                for (int i = 0; i < deltaChi.Count; i++) {
                    pValue.Add(1 - ChiSquaredDistribution.CDF(deltaDf.ElementAt(i), deltaChi.ElementAt(i)));
                }

                return new LikelihoodRatioTestResults() {
                    DeltaChi = deltaChi,
                    DeltaDf = deltaDf,
                    PValue = pValue,
                    DfPolynomial = modelResults.Select(c => c.DfPolynomial).ToList(),
                    LogLikelihood = modelResults.Select(c => c._2LogLikelihood).ToList(),
                    DegreesOfFreedom = modelResults.Select(c => c.DegreesOfFreedom).ToList(),
                };
            }
            return null;
        }

        /// <summary>
        /// Estimates the Anderson-Darling statistic for normality.
        /// According to Dodd: AD = -1 * (1 + 4.0 / X.Length - 25.0 / (Math.Pow(X.Length, 2))) * arg;
        /// Here we use Stephens (1974, 1982, correction for finite sample sizes)
        /// </summary>
        /// <param name="values">Sample values</param>
        public static AndersonDarlingResults AndersonDarling(ICollection<double> values) {
            var sd = Math.Sqrt(values.Variance());
            var mu = values.Average();
            var x = values.SortWithoutNaNs().ToList();
            var adStatistic = 0D;
            var errorRate = 0D;
            var andersonDarlingResults = new AndersonDarlingResults();
            if (x.Count > 1) {
                var Z = new List<double>();

                var critValues = new List<double> { 0, .127, .167, .193, .215, .233, .251, .268, .285, .303, .321, .341, .361, .384, .409, .437, .465, .509, .559, .631, .751, .873, 1.0348, 1.1578 };
                var errorRates = new List<double> { .9999, .99, .95, .90, .85, .80, .75, .70, .65, .60, .55, .50, .45, .40, .35, .30, .25, .20, .15, .10, .05, .025, 0.01, 0.005 };

                foreach (var item in x) {
                    Z.Add(NormalDistribution.CDF(0, 1, (item - mu) / sd));
                }

                var arg = 0D;
                for (int i = 0; i < x.Count; i++) {
                    var L = Z[i] * (1 - Z[x.Count - i - 1]);
                    var lambda = Math.Max(L, 10E-7);
                    arg += (1 + (2 * (i + 1) - 1) * UtilityFunctions.LogBound(lambda) / x.Count);
                }

                adStatistic = -1 * (1 + 0.75 / x.Count - 2.25 / (Math.Pow(x.Count, 2))) * arg;
                if (adStatistic > 0) {
                    for (int i = 0; i < critValues.Count; i++) {
                        if (critValues[i] > adStatistic) {
                            errorRate = errorRates[i - 1];
                            break;
                        }
                    }
                } else {
                    errorRate = double.NaN;
                }
                if (errorRate == 0) {
                    errorRate = 0.005;
                }
            }
            andersonDarlingResults.ADStatistic = adStatistic;
            andersonDarlingResults.ErrorRate = errorRate;
            return andersonDarlingResults;
        }

        /// <summary>
        /// Return degrees of freedom for processing upper uncertainty.
        /// TODO: works for logit, but not for lognormal.
        /// </summary>
        /// <param name="nominal"></param>
        /// <param name="upper"></param>
        /// <param name="nominalUncertaintyUpper"></param>
        /// <param name="upperUncertaintyUpper"></param>
        /// <param name="isLogit"></param>
        /// <returns></returns>
        public static double GetDegreesOfFreedom(double nominal, double upper, double nominalUncertaintyUpper, double upperUncertaintyUpper, bool isLogit) {
            var MAcc = 0D;
            var UAcc = 0D;
            var MUAcc = 0D;
            var UUAcc = 0D;
            if (isLogit) {
                MAcc = UtilityFunctions.Logit(nominal);
                UAcc = UtilityFunctions.Logit(upper);
                MUAcc = UtilityFunctions.Logit(nominalUncertaintyUpper);
                UUAcc = UtilityFunctions.Logit(upperUncertaintyUpper);
            } else {
                MAcc = UtilityFunctions.LogBound(nominal);
                UAcc = UtilityFunctions.LogBound(upper);
                MUAcc = UtilityFunctions.LogBound(nominalUncertaintyUpper);
                UUAcc = UtilityFunctions.LogBound(upperUncertaintyUpper);
            }
            var SAcc = (UAcc - MAcc) / 1.645;
            var SAccAcc = (MUAcc - MAcc) / 1.645;
            var xOpt = BiSectionSearch(df => (getSampleVariance(MAcc, SAcc, SAccAcc, df).Percentile(95) - UUAcc), .1, 1000, 100, 0.001);
            //Do not REMOVE this line
            //var fOpt = getSampleVariance(MAcc, SAcc, SAccAcc, xOpt).Percentile(95);
            return Math.Ceiling(xOpt);
        }

        /// <summary>
        /// A bi-section search algorithm for minimization of single dimensional objective
        /// functions that uses a target objective value (criterium) as stopping condition.
        /// <param name="function">The function that is to be minimized.</param>
        /// <param name="lowerBound">The lower bound of the search space.</param>
        /// <param name="upperBound">The upper bound of the search space.</param>
        /// <param name="iterations">The maximum number of iterations.</param>
        /// <param name="precision">The targetted precision.</param>
        /// <returns></returns>
        public static double BiSectionSearch(
            Func<double, double> function,
            double lowerBound,
            double upperBound,
            int iterations,
            double precision
        ) {
            var xLower = lowerBound;
            var xUpper = upperBound;
            var xMiddle = double.NaN;
            var fLower = function(xLower);
            double fMiddle;
            for (int i = 1; i < iterations; i++) {
                xMiddle = (xLower + xUpper) / 2;
                if ((xUpper - xLower) / xMiddle < precision) {
                    break;
                }
                fMiddle = function(xMiddle);
                if (fLower / fMiddle <= 0) {
                    xUpper = xMiddle;
                } else {
                    xLower = xMiddle;
                    fLower = fMiddle;
                }
            }
            return xMiddle;
        }

        private static List<double> getSampleVariance(double _m, double _s, double __s, double df) {
            var random = new McraRandomGenerator(12345);
            var r = new List<double>();
            for (int i = 0; i < 10000; i++) {
                var m = NormalDistribution.Draw(random, _m, __s);
                var s = _s * Math.Sqrt(ChiSquaredDistribution.Draw(random, df) / df);
                r.Add(m + 1.645 * s);
            }
            return r;
        }

        /// <summary>
        /// A bi-section search algorithm for minimization of single dimensional
        /// objective functions that uses a target objective value (criterium) as
        /// stopping condition.
        /// </summary>
        /// <param name="function">The function that is to be minimized.</param>
        /// <param name="lowerBound">The lower bound of the search space.</param>
        /// <param name="upperBound">The upper bound of the search space.</param>
        /// <param name="iterations">The maximum number of iterations.</param>
        /// <param name="criterium">The targetted function value.</param>
        /// <param name="precision">The targetted precision.</param>
        /// <returns></returns>
        public static double BiSectionSearch(
            Func<double, double> function,
            double lowerBound,
            double upperBound,
            int iterations,
            double criterium,
            double precision
        ) {
            var xLower = lowerBound;
            var xUpper = upperBound;
            var xMiddle = double.NaN;
            double fMiddle;
            for (int i = 0; i < iterations; i++) {
                xMiddle = (xLower + xUpper) / 2;
                fMiddle = function(xMiddle);
                if (Math.Abs(fMiddle - criterium) < precision) {
                    break;
                }
                if (fMiddle > criterium) {
                    xUpper = xMiddle;
                } else {
                    xLower = xMiddle;
                }
            }
            return xMiddle;
        }

        /// <summary>
        /// Snedecor and Cochran, One way classifications, Analysis of Variance, ch 10
        /// </summary>
        /// <param name="means"></param>
        /// <param name="sigma"></param>
        /// <param name="numberOfObservations"></param>
        /// <returns></returns>
        public static FRatioStatistics EqualityOfMeans(
            List<double> means,
            List<double> sigma,
            List<int> numberOfObservations
        ) {
            var ssSum = 0d;
            var s2Sum = 0d;
            var dfTotal = 0;
            var totalSum = 0d;

            if (means.Count <= 1) {
                return new FRatioStatistics();
            }

            for (int i = 0; i < means.Count; i++) {
                var df = numberOfObservations.ElementAt(i) - 1;
                dfTotal += df;
                var total = means.ElementAt(i) * (df + 1);
                totalSum += total;
                ssSum += Math.Pow(sigma.ElementAt(i), 2) * df;
                s2Sum += Math.Pow(total, 2) / (df + 1);
            }
            var msBetween = (s2Sum - Math.Pow(totalSum, 2) / numberOfObservations.Sum()) / (means.Count - 1);
            var msWithin = ssSum / dfTotal;
            var FRatioStatistics = new FRatioStatistics() {
                MsBetween = msBetween,
                MsWithin = msWithin,
                BetweenDf = means.Count - 1,
                WithinDf = dfTotal,
                Probability = 1 - FisherSnedecor.CDF(means.Count - 1, dfTotal, msBetween / msWithin),
            };
            return FRatioStatistics;
        }

        /// <summary>
        /// Snedecor and Cochran, One way classifications, Analysis of Variance, ch 10
        /// </summary>
        /// <param name="sigma"></param>
        /// <param name="numberOfObservations"></param>
        /// <returns></returns>
        public static BartlettsStatistics HomogeneityOfVariances(
            List<double> sigma,
            List<int> numberOfObservations
        ) {
            var fLogS2 = 0d;
            var fReciprocal = 0d;
            var ssSum = 0d;
            var dfTotal = 0;
            for (int i = 0; i < sigma.Count; i++) {
                var df = numberOfObservations.ElementAt(i) - 1;
                fLogS2 += df * Math.Log(Math.Pow(sigma.ElementAt(i), 2));
                ssSum += Math.Pow(sigma.ElementAt(i), 2) * df;
                fReciprocal += 1d / df;
                dfTotal += df;
            }
            var m = dfTotal * Math.Log(ssSum / dfTotal) - fLogS2;
            var c = 1 + 1 / (3d * (sigma.Count - 1)) * (fReciprocal - 1d / dfTotal);
            var bartlettsStatistics = new BartlettsStatistics() {
                C = c,
                M = m,
                Probability = 1 - ChiSquaredDistribution.CDF(sigma.Count - 1, m / c),
            };
            return bartlettsStatistics;
        }

        /// <summary>
        /// Two-sided or one-sided t-test based on Students-distribution.
        /// </summary>
        /// <param name="sample1"></param>
        /// <param name="sample2"></param>
        /// <param name="twoSided"></param>
        /// <returns></returns>
        public static double TTest(List<double> sample1, List<double> sample2, bool twoSided = false) {
            var n1 = sample1.Count;
            var n2 = sample2.Count;
            var mu1 = sample1.Average();
            var mu2 = sample2.Average();
            var ss1 = sample1.Variance();
            var ss2 = sample2.Variance();
            if (n1 <= 2 || n2 <= 2) {
                return double.NaN;
            }
            var ss = ((n1 - 1) * ss1 + (n2 - 1) * ss2) / (n1 + n2 - 2);
            var se = Math.Sqrt(ss * (n1 + n2) / (n1 * n2));
            var t_stat = Math.Abs(mu1 - mu2) / se;
            var student = new StudentT(0, 1, n1 + n2 - 2);
            if (twoSided) {
                return 2 * (1 - student.CumulativeDistribution(t_stat));
            } else {
                return (1 - student.CumulativeDistribution(t_stat));
            }
        }
    }
}
